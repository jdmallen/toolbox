namespace JDMallen.Toolbox.AI;

/// <summary>
/// A provider-agnostic chat completion request: a system prompt, a user prompt,
/// and generation limits. Every <see cref="IChatCompletionClient" /> maps this
/// onto its own wire format.
/// </summary>
/// <param name="SystemPrompt">The system prompt establishing model behavior.</param>
/// <param name="UserPrompt">The user message to complete.</param>
/// <param name="MaxTokens">The maximum number of tokens to generate.</param>
/// <param name="Temperature">
/// The sampling temperature; lower is more
/// deterministic.
/// </param>
public sealed record CompletionRequest(
	string SystemPrompt,
	string UserPrompt,
	int MaxTokens,
	double Temperature = 0.1);
