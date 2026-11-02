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
/// <param name="ApiKey">The Azure OpenAI API key.</param>
/// <param name="Endpoint">
/// The resource endpoint, e.g. "https://myresource.openai.azure.com".
/// </param>
/// <param name="Deployment">The deployment (model) name.</param>
public sealed record AzureOpenAIClientOptions(
	string ApiKey,
	string Endpoint,
	string Deployment)
{
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
