using Azure.Core;
using Azure.Identity;

namespace JDMallen.Toolbox.AI.Azure;

/// <summary>
/// Builds Azure.Identity credentials and bearer token providers from
/// <see cref="AzureCredentialOptions" />.
/// </summary>
/// <remarks>
/// The interactive kinds keep their tokens in a persistent, encrypted cache
/// (the system keyring: libsecret on Linux, Keychain on macOS, DPAPI on
/// Windows) and the sign-in's <c>AuthenticationRecord</c> in
/// <see cref="AzureCredentialOptions.AuthenticationRecordPath" />, so a
/// one-shot CLI prompts once rather than on every run. The cache is never
/// written unencrypted: without a keyring, use
/// <see cref="AzureCredentialKind.AzureCli" /> instead.
/// </remarks>
public static class AzureCredentialFactory
{
	private const string KEYRING_HINT =
		"The persistent token cache needs the system keyring (libsecret on "
		+ "Linux). If none is available, sign in with the Azure CLI instead.";

	/// <summary>Whether <paramref name="kind" /> signs in interactively.</summary>
	public static bool IsInteractive(AzureCredentialKind kind) =>
		kind is AzureCredentialKind.InteractiveBrowser
			or AzureCredentialKind.DeviceCode;

	/// <summary>Builds the credential <paramref name="options" /> describes.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Unknown kind.</exception>
	public static TokenCredential Create(AzureCredentialOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		string? tenantId = TenantOrNull(options);

		return options.Kind switch
		{
			AzureCredentialKind.Default => new DefaultAzureCredential(
				new DefaultAzureCredentialOptions { TenantId = tenantId }),
			AzureCredentialKind.AzureCli => new AzureCliCredential(
				new AzureCliCredentialOptions { TenantId = tenantId }),
			AzureCredentialKind.AzureDeveloperCli =>
				new AzureDeveloperCliCredential(
					new AzureDeveloperCliCredentialOptions { TenantId = tenantId }),
			AzureCredentialKind.InteractiveBrowser =>
				new InteractiveBrowserCredential(
					InteractiveBrowserOptions(options)),
			AzureCredentialKind.DeviceCode => new DeviceCodeCredential(
				DeviceCodeOptions(options)),
			_ => throw new ArgumentOutOfRangeException(
				nameof(options),
				options.Kind,
				"Unknown credential kind."),
		};
	}

	/// <summary>
	/// A cached bearer token provider for <paramref name="options" />, ready for
	/// <see cref="AzureOpenAIClientOptions.ForEntraId" />. For an interactive
	/// kind with no saved sign-in yet, the first request signs in and saves the
	/// <c>AuthenticationRecord</c>, so the next process is silent.
	/// </summary>
	/// <param name="options">Which credential, and its cache settings.</param>
	/// <param name="scope">
	/// The token scope; <see cref="AzureOpenAIScopes.DEFAULT" /> unless set.
	/// </param>
	public static IBearerTokenProvider CreateTokenProvider(
		AzureCredentialOptions options,
		string scope = AzureOpenAIScopes.DEFAULT)
	{
		ArgumentNullException.ThrowIfNull(options);
		TokenCredential credential = Create(options);
		var provider = new TokenCredentialBearerTokenProvider(
			credential,
			scope,
			TenantOrNull(options));

		IBearerTokenProvider inner = IsInteractive(options.Kind)
			? new InteractiveTokenProvider(credential, provider, options)
			: provider;

		return new CachingBearerTokenProvider(inner);
	}

	/// <summary>
	/// Signs in now (for an interactive kind: prompts, then saves the
	/// <c>AuthenticationRecord</c>), so later token requests, such as from a git
	/// hook, need no prompt. For the other kinds, gets one token to prove the
	/// credential works.
	/// </summary>
	/// <param name="options">Which credential, and its cache settings.</param>
	/// <param name="scope">
	/// The token scope; <see cref="AzureOpenAIScopes.DEFAULT" /> unless set.
	/// </param>
	/// <param name="cancellationToken">Cancels the sign-in.</param>
	/// <exception cref="AuthenticationFailedException">Sign-in failed.</exception>
	/// <exception cref="CredentialUnavailableException">
	/// The credential cannot be used here (no keyring, tool not installed).
	/// </exception>
	public static async Task<AzureSignInResult> SignInAsync(
		AzureCredentialOptions options,
		string scope = AzureOpenAIScopes.DEFAULT,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);

		// Signing in is the point here, so prompting is always allowed.
		AzureCredentialOptions interactive =
			options with { AllowInteractiveAuthentication = true };
		TokenCredential credential = Create(interactive);
		var provider = new TokenCredentialBearerTokenProvider(
			credential,
			scope,
			TenantOrNull(interactive));

		if (!IsInteractive(interactive.Kind))
		{
			await provider.GetTokenAsync(cancellationToken).ConfigureAwait(false);

			return new AzureSignInResult(null, null);
		}

		AuthenticationRecord record = await AuthenticateAsync(
				credential,
				provider.RequestContext(),
				interactive,
				cancellationToken)
			.ConfigureAwait(false);

		return new AzureSignInResult(record.Username, record.TenantId);
	}

	/// <summary>Options for the browser credential, with cache and record.</summary>
	internal static InteractiveBrowserCredentialOptions InteractiveBrowserOptions(
		AzureCredentialOptions options) =>
		new()
		{
			TenantId = TenantOrNull(options),
			TokenCachePersistenceOptions = CachePersistence(options),
			AuthenticationRecord = LoadRecord(options),
			DisableAutomaticAuthentication =
				!options.AllowInteractiveAuthentication,
		};

	/// <summary>Options for the device code credential, with cache and record.</summary>
	internal static DeviceCodeCredentialOptions DeviceCodeOptions(AzureCredentialOptions options) =>
		new()
		{
			TenantId = TenantOrNull(options),
			TokenCachePersistenceOptions = CachePersistence(options),
			AuthenticationRecord = LoadRecord(options),
			DisableAutomaticAuthentication =
				!options.AllowInteractiveAuthentication,
		};

	/// <summary>
	/// The saved sign-in, or null when there is none or it cannot be read (a
	/// fresh sign-in then replaces it).
	/// </summary>
	internal static AuthenticationRecord? LoadRecord(AzureCredentialOptions options)
	{
		if (string.IsNullOrWhiteSpace(options.AuthenticationRecordPath)
			|| !File.Exists(options.AuthenticationRecordPath))
		{
			return null;
		}

		try
		{
			using FileStream stream = File.OpenRead(options.AuthenticationRecordPath);

			return AuthenticationRecord.Deserialize(stream);
		}
		catch (Exception exception) when (exception is IOException
											  or UnauthorizedAccessException
											  or System.Text.Json.JsonException
											  or InvalidOperationException)
		{
			return null;
		}
	}

	/// <summary>
	/// Prompts (browser or device code), saves the record when a path is set,
	/// and turns a missing keyring into a message that says what to do.
	/// </summary>
	internal static async Task<AuthenticationRecord> AuthenticateAsync(
		TokenCredential credential,
		TokenRequestContext context,
		AzureCredentialOptions options,
		CancellationToken cancellationToken)
	{
		AuthenticationRecord record;
		try
		{
			record = credential switch
			{
				InteractiveBrowserCredential browser => await browser
					.AuthenticateAsync(context, cancellationToken)
					.ConfigureAwait(false),
				DeviceCodeCredential deviceCode => await deviceCode
					.AuthenticateAsync(context, cancellationToken)
					.ConfigureAwait(false),
				_ => throw new ArgumentException(
					"Only interactive credentials can sign in ahead of time.",
					nameof(credential)),
			};
		}
		catch (CredentialUnavailableException exception)
			when (NeedsKeyringHint(exception))
		{
			throw WithKeyringHint(exception);
		}

		await SaveRecordAsync(record, options, cancellationToken)
			.ConfigureAwait(false);

		return record;
	}

	/// <summary>
	/// Whether <paramref name="exception" /> may be the missing keyring.
	/// <see cref="AuthenticationRequiredException" /> (prompting disabled, no
	/// saved sign-in) derives from it but is not, and keeps its own type so
	/// callers can catch it; nor is one that already has the hint.
	/// </summary>
	internal static bool NeedsKeyringHint(CredentialUnavailableException exception) =>
		exception is not AuthenticationRequiredException
		&& !exception.Message.Contains(KEYRING_HINT, StringComparison.Ordinal);

	internal static CredentialUnavailableException WithKeyringHint(
		CredentialUnavailableException exception) =>
		new($"{exception.Message} {KEYRING_HINT}", exception);

	internal static async Task SaveRecordAsync(
		AuthenticationRecord record,
		AzureCredentialOptions options,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(options.AuthenticationRecordPath))
		{
			return;
		}

		string path = Path.GetFullPath(options.AuthenticationRecordPath);
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		// Write beside, then move, so a crash never leaves half a record.
		var temporary = $"{path}.tmp";
		await using (FileStream stream = File.Create(temporary))
		{
			await record.SerializeAsync(stream, cancellationToken)
				.ConfigureAwait(false);
		}

		File.Move(temporary, path, true);
	}

	private static TokenCachePersistenceOptions CachePersistence(AzureCredentialOptions options) =>
		new()
		{
			Name = string.IsNullOrWhiteSpace(options.CacheName)
				? AzureCredentialOptions.DEFAULT_CACHE_NAME
				: options.CacheName.Trim(),

			// Never fall back to a plain file: no keyring is an error.
			UnsafeAllowUnencryptedStorage = false,
		};

	private static string? TenantOrNull(AzureCredentialOptions options) =>
		string.IsNullOrWhiteSpace(options.TenantId)
			? null
			: options.TenantId.Trim();

	/// <summary>
	/// For an interactive kind: signs in (and saves the record) on the first
	/// request when no saved sign-in exists, then gets tokens silently; and
	/// says what to do when the keyring is missing.
	/// </summary>
	private sealed class InteractiveTokenProvider(
		TokenCredential credential,
		TokenCredentialBearerTokenProvider provider,
		AzureCredentialOptions options) : IBearerTokenProvider
	{
		private bool _signedIn = LoadRecord(options) is not null;

		public async ValueTask<BearerToken> GetTokenAsync(
			CancellationToken cancellationToken = default)
		{
			try
			{
				if (_signedIn
					|| !options.AllowInteractiveAuthentication
					|| string.IsNullOrWhiteSpace(options.AuthenticationRecordPath))
				{
					return await provider.GetTokenAsync(cancellationToken)
						.ConfigureAwait(false);
				}

				await AuthenticateAsync(
						credential,
						provider.RequestContext(),
						options,
						cancellationToken)
					.ConfigureAwait(false);
				_signedIn = true;

				return await provider.GetTokenAsync(cancellationToken)
					.ConfigureAwait(false);
			}
			catch (CredentialUnavailableException exception)
				when (NeedsKeyringHint(exception))
			{
				throw WithKeyringHint(exception);
			}
		}
	}
}
