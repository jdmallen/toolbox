using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Azure OpenAI implementation of <see cref="IChatCompletionClient" />. Owns only
/// the Azure wire format: deployment-in-path URL, <c>api-key</c> header, request
/// body, and the <c>choices[0].message.content</c> response shape.
/// </summary>
/// <param name="httpClient">
/// The HTTP client used for requests. The caller owns its lifetime; requests use
/// absolute URIs, so no base address is required.
/// </param>
/// <param name="options">The API key, endpoint, and deployment to use.</param>
public sealed class AzureOpenAIChatClient(
	HttpClient httpClient,
	AzureOpenAIClientOptions options) : IChatCompletionClient
{
	private const string API_VERSION = "2024-10-21";

	/// <inheritdoc />
	public async Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
	{
		var body = new AzureOpenAIRequest(
			[
				new AzureOpenAIMessage("system", request.SystemPrompt),
				new AzureOpenAIMessage("user", request.UserPrompt),
			],
			request.MaxTokens,
			request.Temperature);

		var url =
			$"{options.Endpoint.TrimEnd('/')}/openai/deployments/{options.Deployment}/chat/completions?api-version={API_VERSION}";

		using var message = new HttpRequestMessage(HttpMethod.Post, url);
		message.Content = JsonContent.Create(
			body,
			AIJsonContext.Default.AzureOpenAIRequest);
		message.Headers.Add("api-key", options.ApiKey);

		HttpResponseMessage response = await httpClient
			.SendAsync(message, cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			string error = await response.Content.ReadAsStringAsync(cancellationToken)
				.ConfigureAwait(false);

			throw new HttpRequestException(
				$"Azure OpenAI API request failed: {response.StatusCode} - {error}");
		}

		AzureOpenAIResponse? result = await response.Content
			.ReadFromJsonAsync(AIJsonContext.Default.AzureOpenAIResponse, cancellationToken)
			.ConfigureAwait(false);

		if (result?.Choices is not { Count: > 0 } choices)
		{
			return string.Empty;
		}

		return choices[0].Message?.Content ?? string.Empty;
	}
}

internal sealed record AzureOpenAIRequest(
	[property: JsonPropertyName("messages")]
	IReadOnlyList<AzureOpenAIMessage> Messages,
	[property: JsonPropertyName("max_tokens")]
	int MaxTokens,
	[property: JsonPropertyName("temperature")]
	double Temperature)
{
	[JsonPropertyName("frequency_penalty")]
	public int FrequencyPenalty { get; init; }

	[JsonPropertyName("presence_penalty")]
	public int PresencePenalty { get; init; }

	[JsonPropertyName("top_p")]
	public double TopP { get; init; } = 1.0;
}

internal sealed record AzureOpenAIMessage(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("content")]
	string Content);

internal sealed record AzureOpenAIResponse(
	[property: JsonPropertyName("choices")]
	IReadOnlyList<AzureOpenAIChoice>? Choices);

internal sealed record AzureOpenAIChoice(
	[property: JsonPropertyName("message")]
	AzureOpenAIChoiceMessage? Message);

internal sealed record AzureOpenAIChoiceMessage(
	[property: JsonPropertyName("content")]
	string? Content);
