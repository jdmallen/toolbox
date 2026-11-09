namespace JDMallen.Toolbox.AI;

/// <summary>
/// Connection settings for <see cref="AnthropicChatClient" />.
/// </summary>
/// <param name="ApiKey">The Anthropic API key.</param>
/// <param name="Model">The model identifier, e.g. "claude-haiku-4-5-20251001".</param>
public sealed record AnthropicClientOptions(string ApiKey, string Model);

/// <summary>
/// Connection settings for <see cref="AzureOpenAIChatClient" />.
/// </summary>
/// <remarks>
/// Authenticates with <see cref="ApiKey" /> (the <c>api-key</c> header) unless
/// <see cref="TokenProvider" /> is set, in which case it sends a Microsoft Entra
/// ID bearer token instead; see <see cref="ForEntraId" />.
/// </remarks>
/// <param name="ApiKey">
/// The Azure OpenAI API key. Ignored when <see cref="TokenProvider" /> is set.
/// </param>
/// <param name="Endpoint">
/// The resource endpoint, e.g. "https://myresource.openai.azure.com" or
/// "https://myresource.services.ai.azure.com". A trailing <c>/openai</c> or
/// <c>/openai/v1</c> pasted from the portal is tolerated.
/// </param>
/// <param name="Deployment">The deployment (model) name.</param>
public sealed record AzureOpenAIClientOptions(
	string ApiKey,
	string Endpoint,
	string Deployment)
{
	/// <summary>
	/// Supplies Microsoft Entra ID bearer tokens. When set, requests carry
	/// <c>Authorization: Bearer</c> and no <c>api-key</c> header, and
	/// <see cref="ApiKey" /> is ignored. Null (the default) uses the API key.
	/// </summary>
	/// <remarks>
	/// The identity needs a data-plane role on the resource, such as "Cognitive
	/// Services OpenAI User", and the endpoint must be the resource's own custom
	/// subdomain. Tokens are only sent over https (or http to loopback).
	/// </remarks>
	public IBearerTokenProvider? TokenProvider { get; init; }

	/// <summary>
	/// Options that authenticate with Microsoft Entra ID tokens instead of an API
	/// key.
	/// </summary>
	/// <param name="endpoint">The resource endpoint.</param>
	/// <param name="deployment">The deployment (model) name.</param>
	/// <param name="tokenProvider">
	/// Supplies the tokens, e.g. a <see cref="CachingBearerTokenProvider" /> over
	/// an <see cref="AzureCliTokenProvider" />.
	/// </param>
	/// <returns>Options with <see cref="TokenProvider" /> set and no API key.</returns>
	public static AzureOpenAIClientOptions ForEntraId(
		string endpoint,
		string deployment,
		IBearerTokenProvider tokenProvider)
	{
		ArgumentNullException.ThrowIfNull(tokenProvider);

		return new AzureOpenAIClientOptions(string.Empty, endpoint, deployment)
		{
			TokenProvider = tokenProvider,
		};
	}

	/// <summary>
	/// Whether the deployment is a reasoning model (o-series, gpt-5 and later
	/// reasoning deployments). Reasoning models reject sampling parameters, so
	/// <see cref="CompletionRequest.Temperature" /> is not sent when this is
	/// true. Reasoning tokens also come out of
	/// <see cref="CompletionRequest.MaxTokens" />, so such a deployment usually
	/// needs a larger budget.
	/// </summary>
	/// <remarks>
	/// An init property rather than a constructor parameter, so adding it did not
	/// change the constructor callers compiled against.
	/// </remarks>
	public bool IsReasoningModel { get; init; }
}

/// <summary>
/// Connection settings for <see cref="OpenAICompatibleChatClient" />.
/// </summary>
/// <param name="BaseUrl">
/// The API root, up to and including the version segment and excluding
/// <c>/chat/completions</c> — e.g. "https://api.openai.com/v1" or
/// "http://10.10.0.20:8080/v1". A trailing slash is tolerated.
/// </param>
/// <param name="Model">
/// The model identifier the server expects, e.g. "gpt-4o-mini" or a local
/// runner's model name.
/// </param>
/// <param name="ApiKey">
/// The bearer token, or <see langword="null" /> for servers that do not
/// authenticate — the common case for self-hosted runners. When null, empty, or
/// whitespace, no <c>Authorization</c> header is sent.
/// </param>
public sealed record OpenAICompatibleClientOptions(
	string BaseUrl,
	string Model,
	string? ApiKey = null);
