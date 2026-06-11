namespace JDMallen.Toolbox.AI;

/// <summary>
///     Connection settings for <see cref="AnthropicChatClient" />.
/// </summary>
/// <param name="ApiKey">The Anthropic API key.</param>
/// <param name="Model">The model identifier, e.g. "claude-haiku-4-5-20251001".</param>
public sealed record AnthropicClientOptions(string ApiKey, string Model);

/// <summary>
///     Connection settings for <see cref="AzureOpenAIChatClient" />.
/// </summary>
/// <param name="ApiKey">The Azure OpenAI API key.</param>
/// <param name="Endpoint">
///     The resource endpoint, e.g. "https://myresource.openai.azure.com".
/// </param>
/// <param name="Deployment">The deployment (model) name.</param>
public sealed record AzureOpenAIClientOptions(
	string ApiKey,
	string Endpoint,
	string Deployment);
