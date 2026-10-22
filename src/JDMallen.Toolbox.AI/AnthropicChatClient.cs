using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Anthropic (Claude) implementation of <see cref="IChatCompletionClient" />.
/// Owns only the Anthropic wire format: the Messages API endpoint,
/// <c>x-api-key</c> + <c>anthropic-version</c> headers, a top-level
/// <c>system</c> prompt, and the <c>content[].text</c> response shape.
/// </summary>
/// <param name="httpClient">
/// The HTTP client used for requests. The caller owns its lifetime; requests use
/// absolute URIs, so no base address is required.
/// </param>
/// <param name="options">The API key and model to use.</param>
public sealed class AnthropicChatClient(
	HttpClient httpClient,
	AnthropicClientOptions options) : IChatCompletionClient
{
	private const string URL = "https://api.anthropic.com/v1/messages";
	private const string ANTHROPIC_VERSION = "2023-06-01";

	/// <inheritdoc />
	public async Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
	{
		var body = new AnthropicRequest(
			options.Model,
			request.MaxTokens,
			request.Temperature,
			request.SystemPrompt,
			[new AnthropicMessage("user", BuildUserContent(request))]);

		using var message = new HttpRequestMessage(HttpMethod.Post, URL);
		message.Content = JsonContent.Create(
			body,
			AIJsonContext.Default.AnthropicRequest);
		message.Headers.Add("x-api-key", options.ApiKey);
		message.Headers.Add("anthropic-version", ANTHROPIC_VERSION);

		HttpResponseMessage response = await httpClient
			.SendAsync(message, cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			string error = await response.Content.ReadAsStringAsync(cancellationToken)
				.ConfigureAwait(false);

			throw new HttpRequestException(
				$"Anthropic API request failed: {response.StatusCode} - {error}");
		}

		AnthropicResponse? result = await response.Content
			.ReadFromJsonAsync(AIJsonContext.Default.AnthropicResponse, cancellationToken)
			.ConfigureAwait(false);

		// The Messages API returns an array of content blocks; concatenate the text ones.
		var sb = new StringBuilder();

		foreach (AnthropicBlock block in (result?.Content ?? [])
		         .Where(block => block.Type == "text"))
		{
			sb.Append(block.Text);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Returns the prompt as a plain string when there are no images, keeping the
	/// original wire format for text-only requests. With images, returns an array
	/// of content blocks: each image as a base64 <c>image</c> block, then the
	/// prompt as a <c>text</c> block (Anthropic recommends images first).
	/// </summary>
	/// <remarks>
	/// An explicit array rather than a collection expression, so the runtime type
	/// is the one registered in <see cref="AIJsonContext" />.
	/// </remarks>
	private static object BuildUserContent(CompletionRequest request)
	{
		if (request.Images.Count == 0)
		{
			return request.UserPrompt;
		}

		var blocks = new AnthropicContentBlock[request.Images.Count + 1];

		for (var index = 0; index < request.Images.Count; index++)
		{
			ImageInput image = request.Images[index];

			blocks[index] = new AnthropicContentBlock("image")
			{
				Source = new AnthropicImageSource(
					"base64",
					image.MediaType,
					image.ToBase64()),
			};
		}

		blocks[^1] = new AnthropicContentBlock("text") { Text = request.UserPrompt };

		return blocks;
	}
}

internal sealed record AnthropicRequest(
	[property: JsonPropertyName("model")] string Model,
	[property: JsonPropertyName("max_tokens")]
	int MaxTokens,
	[property: JsonPropertyName("temperature")]
	double Temperature,
	[property: JsonPropertyName("system")] string System,
	[property: JsonPropertyName("messages")]
	IReadOnlyList<AnthropicMessage> Messages);

// Content is a string, or an AnthropicContentBlock[] when images are attached.
internal sealed record AnthropicMessage(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("content")]
	object Content);

internal sealed record AnthropicContentBlock([property: JsonPropertyName("type")] string Type)
{
	[JsonPropertyName("source")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public AnthropicImageSource? Source { get; init; }

	[JsonPropertyName("text")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Text { get; init; }
}

internal sealed record AnthropicImageSource(
	[property: JsonPropertyName("type")] string Type,
	[property: JsonPropertyName("media_type")]
	string MediaType,
	[property: JsonPropertyName("data")] string Data);

internal sealed record AnthropicResponse(
	[property: JsonPropertyName("content")]
	IReadOnlyList<AnthropicBlock>? Content);

internal sealed record AnthropicBlock(
	[property: JsonPropertyName("type")] string? Type,
	[property: JsonPropertyName("text")] string? Text);
