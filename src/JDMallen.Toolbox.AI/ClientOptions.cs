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
	string Deployment);

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
