using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Gets Microsoft Entra ID tokens from the Azure CLI by running
/// <c>az account get-access-token --scope &lt;scope&gt; [--tenant &lt;tenant&gt;] --output json</c>,
/// so the signed-in <c>az login</c> session (with its MFA and Conditional Access)
/// authenticates the app. No Azure SDK dependency, trim- and AOT-safe.
/// </summary>
/// <remarks>
/// <para>
/// Each call starts an <c>az</c> process (a few hundred milliseconds). Wrap it in
/// a <see cref="CachingBearerTokenProvider" /> so a long-lived client reuses the
/// token until it nears expiry.
/// </para>
/// <para>
/// Requires Azure CLI 2.54 or later, which reports <c>expires_on</c>. Failures
/// throw <see cref="AzureCliCredentialException" /> with a message saying what to
/// do; the token itself never appears in a message.
/// </para>
/// </remarks>
public sealed partial class AzureCliTokenProvider : IBearerTokenProvider
{
	private const int MAX_ERROR_DETAIL_LENGTH = 1000;

	// cmd.exe's exit code for "is not recognized as an internal or external command".
	private const int WINDOWS_COMMAND_NOT_FOUND_EXIT_CODE = 9009;

	private readonly AzureCliTokenProviderOptions _options;
	private readonly IProcessRunner _processRunner;
	private readonly bool _isWindows;

	/// <summary>Creates a provider with the given options.</summary>
	/// <param name="options">
	/// Scope, tenant, timeout, and <c>az</c> location; defaults when null.
	/// </param>
	/// <exception cref="ArgumentException">
	/// The scope, tenant, or timeout is not valid.
	/// </exception>
	public AzureCliTokenProvider(AzureCliTokenProviderOptions? options = null)
		: this(
			options ?? new AzureCliTokenProviderOptions(),
			ProcessRunner.Instance,
			OperatingSystem.IsWindows())
	{
	}

	internal AzureCliTokenProvider(
		AzureCliTokenProviderOptions options,
		IProcessRunner processRunner,
		bool isWindows)
	{
		ArgumentNullException.ThrowIfNull(options);
		Validate(options);

		_options = options;
		_processRunner = processRunner;
		_isWindows = isWindows;
	}

	/// <inheritdoc />
	/// <exception cref="AzureCliCredentialException">
	/// <c>az</c> is missing, not signed in, timed out, or returned no usable token.
	/// </exception>
	public async ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default)
	{
		(string fileName, IReadOnlyList<string> arguments) = BuildCommand();

		ProcessResult result;
		try
		{
			result = await _processRunner
				.RunAsync(
					fileName,
					arguments,
					_options.Timeout,
					cancellationToken)
				.ConfigureAwait(false);
		}
		catch (Win32Exception exception)
		{
			throw new AzureCliCredentialException(NotFoundMessage(), exception);
		}
		catch (TimeoutException exception)
		{
			throw new AzureCliCredentialException(
				$"Azure CLI did not return a token within {_options.Timeout.TotalSeconds:0.#} seconds.",
				exception);
		}

		return result.ExitCode != 0
			? throw new AzureCliCredentialException(DescribeFailure(result))
			: ParseToken(result.StandardOutput);
	}

	/// <summary>The executable and arguments to run, for this platform.</summary>
	internal (string FileName, IReadOnlyList<string> Arguments) BuildCommand()
	{
		var azArguments = new List<string>
		{
			"account",
			"get-access-token",
			"--scope",
			_options.Scope,
		};

		if (!string.IsNullOrWhiteSpace(_options.TenantId))
		{
			azArguments.Add("--tenant");
			azArguments.Add(_options.TenantId);
		}

		azArguments.Add("--output");
		azArguments.Add("json");

		string az = string.IsNullOrWhiteSpace(_options.AzExecutablePath)
			? "az"
			: _options.AzExecutablePath;

		// On Windows `az` is az.cmd, a batch file Process.Start cannot launch
		// directly, so it goes through cmd.exe. Arguments are validated to a
		// character set cmd.exe treats literally.
		bool viaCommandShell = _isWindows
			&& (az == "az"
				|| az.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)
				|| az.EndsWith(".bat", StringComparison.OrdinalIgnoreCase));

		return !viaCommandShell ? (az, azArguments) : ("cmd.exe", ["/d", "/c", az, .. azArguments]);
	}

	private static void Validate(AzureCliTokenProviderOptions options)
	{
		if (string.IsNullOrWhiteSpace(options.Scope)
			|| !ScopePattern().IsMatch(options.Scope))
		{
			throw new ArgumentException(
				$"Scope must be an https URL such as \"{AzureOpenAIScopes.DEFAULT}\".",
				nameof(options));
		}

		if (!string.IsNullOrWhiteSpace(options.TenantId)
			&& !TenantPattern().IsMatch(options.TenantId))
		{
			throw new ArgumentException(
				"TenantId must be a tenant GUID or domain name.",
				nameof(options));
		}

		if (options.Timeout <= TimeSpan.Zero)
		{
			throw new ArgumentException(
				"Timeout must be greater than zero.",
				nameof(options));
		}
	}

	private BearerToken ParseToken(string standardOutput)
	{
		AzureCliTokenResponse? response;
		try
		{
			response = JsonSerializer.Deserialize(
				standardOutput,
				AIJsonContext.Default.AzureCliTokenResponse);
		}
		catch (JsonException exception)
		{
			// The output is deliberately not quoted: it may contain the token.
			throw new AzureCliCredentialException(
				"Azure CLI returned output that is not a token response.",
				exception);
		}

		if (string.IsNullOrEmpty(response?.AccessToken))
		{
			throw new AzureCliCredentialException(
				$"Azure CLI returned no access token for scope {_options.Scope}.");
		}

		if (response.ExpiresOn is not { } expiresOn)
		{
			throw new AzureCliCredentialException(
				"Azure CLI did not report the token's expiry (expires_on). "
				+ "Upgrade Azure CLI to 2.54 or later.");
		}

		return new BearerToken(
			response.AccessToken,
			DateTimeOffset.FromUnixTimeSeconds(expiresOn));
	}

	private string DescribeFailure(ProcessResult result)
	{
		string error = result.StandardError.Trim();

		if (_isWindows
			&& (result.ExitCode == WINDOWS_COMMAND_NOT_FOUND_EXIT_CODE
				|| error.Contains("is not recognized", StringComparison.OrdinalIgnoreCase)))
		{
			return NotFoundMessage();
		}

		string detail = error.Length > MAX_ERROR_DETAIL_LENGTH
			? error[..MAX_ERROR_DETAIL_LENGTH] + "…"
			: error;

		bool needsSignIn =
			error.Contains("az login", StringComparison.OrdinalIgnoreCase)
			|| error.Contains("AADSTS", StringComparison.Ordinal);

		string summary = needsSignIn
			? $"Azure CLI could not get a token for scope {_options.Scope}. "
			+ $"Sign in with: {SignInCommand()}"
			: $"Azure CLI exited with code {result.ExitCode} while getting a token "
			+ $"for scope {_options.Scope}.";

		return detail.Length == 0
			? summary
			: $"{summary}{Environment.NewLine}Azure CLI said: {detail}";
	}

	private string SignInCommand() =>
		string.IsNullOrWhiteSpace(_options.TenantId)
			? $"az login --scope {_options.Scope}"
			: $"az login --tenant {_options.TenantId} --scope {_options.Scope}";

	private string NotFoundMessage() =>
		string.IsNullOrWhiteSpace(_options.AzExecutablePath)
			? "Azure CLI ('az') was not found on PATH. Install it, or set the path "
			+ "to the az executable."
			: $"Azure CLI was not found at '{_options.AzExecutablePath}'.";

	// An https URL of unreserved characters only: nothing cmd.exe or az could
	// read as an operator or option.
	[GeneratedRegex("^https://[A-Za-z0-9.-]+(?::[0-9]{1,5})?(?:/[A-Za-z0-9._~-]*)*$")]
	private static partial Regex ScopePattern();

	// A tenant GUID or a domain name such as contoso.onmicrosoft.com.
	[GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9.-]{0,252}$")]
	private static partial Regex TenantPattern();
}

/// <summary>Settings for <see cref="AzureCliTokenProvider" />.</summary>
public sealed record AzureCliTokenProviderOptions
{
	/// <summary>
	/// The token scope; <see cref="AzureOpenAIScopes.DEFAULT" /> unless set.
	/// </summary>
	public string Scope { get; init; } = AzureOpenAIScopes.DEFAULT;

	/// <summary>
	/// The Entra tenant to get the token from, as a GUID or domain name. Null uses
	/// the tenant of the current <c>az</c> account.
	/// </summary>
	public string? TenantId { get; init; }

	/// <summary>How long to wait for <c>az</c>; 30 seconds unless set.</summary>
	public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

	/// <summary>
	/// The full path to the <c>az</c> executable, for when it is not on the
	/// PATH the app runs with (e.g. GUI-launched git hooks). Null resolves
	/// <c>az</c> from PATH.
	/// </summary>
	public string? AzExecutablePath { get; init; }
}

/// <summary>
/// The Azure CLI could not supply a token. The message says what to do (install
/// <c>az</c>, run <c>az login</c>, upgrade) and never contains the token.
/// </summary>
public sealed class AzureCliCredentialException : InvalidOperationException
{
	/// <summary>Creates the exception with a default message.</summary>
	public AzureCliCredentialException()
		: base("Azure CLI could not supply a token.")
	{
	}

	/// <summary>Creates the exception with a message.</summary>
	/// <param name="message">What went wrong and what to do about it.</param>
	public AzureCliCredentialException(string message)
		: base(message)
	{
	}

	/// <summary>Creates the exception with a message and its cause.</summary>
	/// <param name="message">What went wrong and what to do about it.</param>
	/// <param name="innerException">The underlying failure.</param>
	public AzureCliCredentialException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}

/// <summary>The fields read from <c>az account get-access-token --output json</c>.</summary>
internal sealed record AzureCliTokenResponse(
	[property: JsonPropertyName("accessToken")]
	string? AccessToken,
	[property: JsonPropertyName("expires_on")]
	[property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	long? ExpiresOn);
