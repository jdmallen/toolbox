using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Azure OpenAI implementation of <see cref="IChatCompletionClient" />. Owns only
/// the Azure wire format: deployment-in-path URL, <c>api-key</c> header (or an
/// Entra ID bearer token, see <see cref="AzureOpenAIClientOptions.TokenProvider" />),
/// request body, and the <c>choices[0].message.content</c> response shape.
/// </summary>
/// <remarks>
/// The budget is sent as <c>max_completion_tokens</c>, which Azure accepts from
/// every chat model; the deprecated <c>max_tokens</c> is rejected by reasoning
/// models. <c>top_p</c> and the penalties are left at the service defaults, and
/// <c>temperature</c> is omitted for
/// <see cref="AzureOpenAIClientOptions.IsReasoningModel" /> deployments, because
/// reasoning models reject sampling parameters.
/// </remarks>
/// <param name="httpClient">
/// The HTTP client used for requests. The caller owns its lifetime; requests use
/// absolute URIs, so no base address is required.
/// </param>
/// <param name="options">
/// The credentials (API key or token provider), endpoint, and deployment to use.
/// </param>
public sealed class AzureOpenAIChatClient(
	HttpClient httpClient,
	AzureOpenAIClientOptions options) : IChatCompletionClient
{
	private const string API_VERSION = "2024-10-21";

	private const string ENTRA_ID_HINT =
		" (Entra ID auth: the signed-in identity needs a data-plane role such as "
		+ "\"Cognitive Services OpenAI User\" or \"Cognitive Services User\" on the "
		+ "resource, and new assignments take up to 5 minutes to apply. The "
		+ "endpoint must be the resource's custom subdomain, and the token's scope "
		+ "must be one the resource accepts.)";

	// Portal-copied endpoints sometimes carry the API root; the client appends
	// its own, so a pasted one is removed rather than doubled.
	private static readonly string[] PastedApiRoots = ["/openai/v1", "/openai"];

	/// <inheritdoc />
	[Obsolete(
		"Use GetCompletionAsync, which also returns token usage and response "
		+ "metadata. CompleteAsync will be removed in the next major version.")]
	public async Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
		=> (await GetCompletionAsync(request, cancellationToken)
			.ConfigureAwait(false)).Text;

	/// <inheritdoc />
	public async Task<CompletionResult> GetCompletionAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
	{
		var body = new AzureOpenAIRequest(
			[
				new AzureOpenAIMessage("system", request.SystemPrompt),
				new AzureOpenAIMessage("user", OpenAIContentParts.Build(request)),
			],
			request.MaxTokens,
			options.IsReasoningModel ? null : request.Temperature);

		string endpoint = NormalizeEndpoint(options.Endpoint);
		var url =
			$"{endpoint}/openai/deployments/{options.Deployment}/chat/completions?api-version={API_VERSION}";

		using var message = new HttpRequestMessage(HttpMethod.Post, url);
		message.Content = JsonContent.Create(
			body,
			AIJsonContext.Default.AzureOpenAIRequest);

		if (options.TokenProvider is { } tokenProvider)
		{
			EnsureSafeForBearerToken(message.RequestUri!);

			BearerToken token = await tokenProvider
				.GetTokenAsync(cancellationToken)
				.ConfigureAwait(false);

			message.Headers.Authorization =
				new AuthenticationHeaderValue("Bearer", token.Value);
		}
		else
		{
			message.Headers.Add("api-key", options.ApiKey);
		}

		HttpResponseMessage response = await httpClient
			.SendAsync(message, cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			string error = await response.Content.ReadAsStringAsync(cancellationToken)
				.ConfigureAwait(false);

			bool isAuthFailure =
				response.StatusCode is HttpStatusCode.Unauthorized
					or HttpStatusCode.Forbidden;
			string hint = options.TokenProvider is not null && isAuthFailure
				? ENTRA_ID_HINT
				: string.Empty;

			throw new HttpRequestException(
				$"Azure OpenAI API request failed: {response.StatusCode} - {error}{hint}",
				null,
				response.StatusCode);
		}

		AzureOpenAIResponse? result = await response.Content
			.ReadFromJsonAsync(AIJsonContext.Default.AzureOpenAIResponse, cancellationToken)
			.ConfigureAwait(false);

		AzureOpenAIChoice? firstChoice =
			result?.Choices is { Count: > 0 } choices ? choices[0] : null;

		return new CompletionResult(firstChoice?.Message?.Content ?? string.Empty)
		{
			Usage = result?.Usage?.ToTokenUsage(),
			Model = result?.Model,
			Id = result?.Id,
			StopReason = firstChoice?.FinishReason,
		};
	}

	/// <summary>
	/// Trims trailing slashes and a pasted API root (<c>/openai</c>,
	/// <c>/openai/v1</c>). Any other path is kept, so endpoints behind a gateway
	/// with a path prefix (e.g. API Management) still work.
	/// </summary>
	internal static string NormalizeEndpoint(string endpoint)
	{
		string trimmed = endpoint.TrimEnd('/');

		foreach (string apiRoot in PastedApiRoots.Where(apiRoot
			         => trimmed.EndsWith(apiRoot, StringComparison.OrdinalIgnoreCase)))
		{
			return trimmed[..^apiRoot.Length].TrimEnd('/');
		}

		return trimmed;
	}

	// A bearer token is a credential for every resource that accepts its scope,
	// not just this one; never send it in clear text off the machine.
	private static void EnsureSafeForBearerToken(Uri requestUri)
	{
		if (requestUri.Scheme != Uri.UriSchemeHttps && !requestUri.IsLoopback)
		{
			throw new InvalidOperationException(
				$"Refusing to send an Entra ID token over {requestUri.Scheme}; "
				+ "use an https endpoint.");
		}
	}
}

internal sealed record AzureOpenAIRequest(
	[property: JsonPropertyName("messages")]
	IReadOnlyList<AzureOpenAIMessage> Messages,
	[property: JsonPropertyName("max_completion_tokens")]
	int MaxCompletionTokens,
	[property: JsonPropertyName("temperature")]
	[property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	double? Temperature);

// Content is a string, or an OpenAIContentPart[] when images are attached.
internal sealed record AzureOpenAIMessage(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("content")]
	object Content);

internal sealed record AzureOpenAIResponse(
	[property: JsonPropertyName("choices")]
	IReadOnlyList<AzureOpenAIChoice>? Choices)
{
	[JsonPropertyName("id")]
	public string? Id { get; init; }

	[JsonPropertyName("model")]
	public string? Model { get; init; }

	[JsonPropertyName("usage")]
	public OpenAIUsage? Usage { get; init; }
}

internal sealed record AzureOpenAIChoice(
	[property: JsonPropertyName("message")]
	AzureOpenAIChoiceMessage? Message)
{
	[JsonPropertyName("finish_reason")]
	public string? FinishReason { get; init; }
}

internal sealed record AzureOpenAIChoiceMessage(
	[property: JsonPropertyName("content")]
	string? Content);
