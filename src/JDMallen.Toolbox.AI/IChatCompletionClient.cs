namespace JDMallen.Toolbox.AI;

/// <summary>
/// The single seam every LLM provider implements: given a
/// <see cref="CompletionRequest" />, return the model's completion. Provider-
/// specific concerns (endpoint, auth, request/response shape) live in
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
	[Obsolete(
		"Use GetCompletionAsync, which also returns token usage and response "
		+ "metadata. CompleteAsync will be removed in the next major version.")]
	Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends the request to the provider and returns the completion text along
	/// with the token usage and metadata the provider reported.
	/// </summary>
	/// <remarks>
	/// The default implementation wraps <see cref="CompleteAsync" /> and reports
	/// no metadata, so implementations written before this member existed keep
	/// compiling. Every built-in client overrides it.
	/// </remarks>
	/// <param name="request">The provider-agnostic completion request.</param>
	/// <param name="cancellationToken">Cancels the underlying HTTP call.</param>
	/// <returns>The completion and its metadata.</returns>
	/// <exception cref="HttpRequestException">
	/// Thrown when the provider returns a non-success status code.
	/// </exception>
	async Task<CompletionResult> GetCompletionAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
	{
#pragma warning disable CS0618 // The fallback for pre-existing implementations.
		string text = await CompleteAsync(request, cancellationToken)
			.ConfigureAwait(false);
#pragma warning restore CS0618

		return new CompletionResult(text);
	}
}
