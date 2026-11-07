using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// The <c>usage</c> object of an OpenAI-style chat completion response, shared
/// by Azure OpenAI and OpenAI-compatible servers. <c>prompt_tokens</c> already
/// includes any cached prompt tokens, and <c>completion_tokens</c> any
/// reasoning tokens.
/// </summary>
internal sealed record OpenAIUsage
{
	[JsonPropertyName("prompt_tokens")]
	public int PromptTokens { get; init; }

	[JsonPropertyName("completion_tokens")]
	public int CompletionTokens { get; init; }

	public TokenUsage ToTokenUsage() => new(PromptTokens, CompletionTokens);
}
