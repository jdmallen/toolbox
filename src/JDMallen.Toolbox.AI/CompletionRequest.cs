namespace JDMallen.Toolbox.AI;

/// <summary>
/// A provider-agnostic chat completion request: a system prompt, a user prompt,
/// optional images, and generation limits. Every
/// <see cref="IChatCompletionClient" /> maps this onto its own wire format.
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
	double Temperature = 0.1)
{
	/// <summary>
	/// Images sent with the user prompt, ahead of its text, for vision-capable
	/// models. Empty by default, in which case the user message is sent as plain
	/// text exactly as it was before image support existed.
	/// </summary>
	/// <remarks>
	/// An init property rather than a constructor parameter, so adding it did not
	/// change the constructor callers compiled against.
	/// </remarks>
	public IReadOnlyList<ImageInput> Images { get; init; } = [];
}
