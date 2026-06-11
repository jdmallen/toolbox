namespace JDMallen.Toolbox.AI;

/// <summary>
/// The single seam every LLM provider implements: given a
/// <see cref="CompletionRequest" />, return the model's raw completion text.
/// Provider-specific concerns (endpoint, auth, request/response shape) live in
/// implementations. Prompt construction and response parsing deliberately live
/// outside this interface, so adding a provider never means re-implementing them.
/// </summary>
public interface IChatCompletionClient
{
	/// <summary>
	/// Sends the request to the provider and returns the raw completion text.
	/// </summary>
	/// <param name="request">The provider-agnostic completion request.</param>
	/// <param name="cancellationToken">Cancels the underlying HTTP call.</param>
	/// <returns>The model's completion text, or an empty string if none.</returns>
	/// <exception cref="HttpRequestException">
	/// Thrown when the provider returns a non-success status code.
	/// </exception>
	Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default);
}
