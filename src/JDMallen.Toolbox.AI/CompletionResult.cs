namespace JDMallen.Toolbox.AI;

/// <summary>
/// The outcome of a chat completion: the text, plus whatever metadata the
/// provider reported alongside it. Every metadata member is optional, because
/// not every server reports every field (self-hosted runners in particular may
/// omit usage).
/// </summary>
/// <param name="Text">The model's completion text, or an empty string if none.</param>
public sealed record CompletionResult(string Text)
{
	/// <summary>
	/// Token counts for this request, or null when the provider reported none.
	/// </summary>
	public TokenUsage? Usage { get; init; }

	/// <summary>
	/// The model that answered, as the provider named it in the response (which
	/// may be more specific than the model requested, e.g. a dated snapshot), or
	/// null when the response did not say.
	/// </summary>
	public string? Model { get; init; }

	/// <summary>
	/// The provider's identifier for this response (Anthropic <c>id</c>, OpenAI
	/// <c>id</c>), or null when the response had none.
	/// </summary>
	public string? Id { get; init; }

	/// <summary>
	/// Why generation stopped, as the provider's raw value: Anthropic
	/// <c>stop_reason</c> (<c>end_turn</c>, <c>max_tokens</c>, ...) or OpenAI
	/// <c>finish_reason</c> (<c>stop</c>, <c>length</c>, ...). Null when the
	/// response did not say. A truncated answer is the usual reason to look.
	/// </summary>
	public string? StopReason { get; init; }
}

/// <summary>
/// Token counts for one completion request, normalized across providers.
/// </summary>
/// <param name="InputTokens">
/// Every prompt token the request was billed for. For Anthropic this sums
/// <c>input_tokens</c>, <c>cache_creation_input_tokens</c>, and
/// <c>cache_read_input_tokens</c>, so it means the same thing as OpenAI's
/// <c>prompt_tokens</c>, which already includes cached tokens.
/// </param>
/// <param name="OutputTokens">
/// Generated tokens (Anthropic <c>output_tokens</c>, OpenAI
/// <c>completion_tokens</c>), including any hidden reasoning tokens.
/// </param>
public sealed record TokenUsage(int InputTokens, int OutputTokens)
{
	/// <summary><see cref="InputTokens" /> plus <see cref="OutputTokens" />.</summary>
	public int TotalTokens => InputTokens + OutputTokens;
}
