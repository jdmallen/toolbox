using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Implementation of <see cref="IChatCompletionClient" /> for any server that
/// speaks the OpenAI <c>/v1/chat/completions</c> API — openai.com itself, or a
/// self-hosted runner such as llama.cpp, llama-swap, Ollama, vLLM, LM Studio, or
/// text-generation-webui. Owns only that wire format: <c>{BaseUrl}/chat/completions</c>,
/// an optional <c>Authorization: Bearer</c> header, the model named in the request
/// body, and the <c>choices[0].message.content</c> response shape.
/// </summary>
/// <remarks>
/// Two accommodations exist for locally hosted reasoning ("thinking") models,
/// which are common on self-hosted runners and otherwise return unusable text:
/// the <c>reasoning_content</c> field some runners emit is ignored, and inline
/// <c>&lt;think&gt;</c> blocks are stripped from the content. See
/// <see cref="StripReasoning" /> for the truncation case.
/// </remarks>
/// <param name="httpClient">
/// The HTTP client used for requests. The caller owns its lifetime; requests use
/// absolute URIs, so no base address is required. Note that a cold self-hosted
/// model can take minutes to load and generate, so give the client a timeout to
/// match — the default <see cref="HttpClient" /> timeout of 100 seconds is often
/// too short.
/// </param>
/// <param name="options">The base URL, model, and optional API key to use.</param>
public sealed class OpenAICompatibleChatClient(
	HttpClient httpClient,
	OpenAICompatibleClientOptions options) : IChatCompletionClient
{
	private static readonly string[] ReasoningTagNames = ["think", "thinking", "reasoning"];

	/// <inheritdoc />
	public async Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default)
	{
		var body = new OpenAICompatibleRequest(
			options.Model,
			[
				new OpenAICompatibleMessage("system", request.SystemPrompt),
				new OpenAICompatibleMessage("user", OpenAIContentParts.Build(request)),
			],
			request.MaxTokens,
			request.Temperature);

		var url = $"{options.BaseUrl.TrimEnd('/')}/chat/completions";

		using var message = new HttpRequestMessage(HttpMethod.Post, url);
		message.Content = JsonContent.Create(
			body,
			AIJsonContext.Default.OpenAICompatibleRequest);

		// Local runners typically accept anonymous requests; only authenticate
		// when a key was actually supplied, so an empty key is not sent as one.
		if (!string.IsNullOrWhiteSpace(options.ApiKey))
		{
			message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
		}

		HttpResponseMessage response = await httpClient
			.SendAsync(message, cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccessStatusCode)
		{
			string error = await response.Content.ReadAsStringAsync(cancellationToken)
				.ConfigureAwait(false);

			throw new HttpRequestException(
				$"OpenAI-compatible API request failed: {response.StatusCode} - {error}");
		}

		OpenAICompatibleResponse? result = await response.Content
			.ReadFromJsonAsync(AIJsonContext.Default.OpenAICompatibleResponse, cancellationToken)
			.ConfigureAwait(false);

		// reasoning_content, when present, is deliberately dropped: callers asked
		// for the completion, not the model's scratch work.
		return result?.Choices is not { Count: > 0 } choices
			? string.Empty
			: StripReasoning(choices[0].Message?.Content ?? string.Empty);
	}

	/// <summary>
	/// Removes inline reasoning blocks that thinking models emit into the content
	/// when the server is not configured to split them into
	/// <c>reasoning_content</c>.
	/// </summary>
	/// <remarks>
	/// A <em>leading</em> unclosed opening tag means generation was truncated
	/// mid-thought (usually by <see cref="CompletionRequest.MaxTokens" />), so
	/// everything from that tag on is discarded rather than returned as an answer:
	/// reasoning passed off as a completion is worse than no completion, and an
	/// empty string is a result callers already handle. An unclosed tag anywhere
	/// else is treated as literal text — it is far more likely to be part of the
	/// answer (a shell command, a snippet, an XML sample) than a stray thought.
	/// </remarks>
	private static string StripReasoning(string rawContent)
	{
		string content = rawContent.Trim();
		if (content.Length == 0)
		{
			return content;
		}

		var stripped = new StringBuilder(content.Length);
		var cursor = 0;

		while (cursor < content.Length)
		{
			int openStart = -1;
			int openEnd = -1;

			foreach (string tagName in ReasoningTagNames)
			{
				int candidate = content.IndexOf(
					$"<{tagName}>",
					cursor,
					StringComparison.OrdinalIgnoreCase);

				if (candidate < 0 || (openStart >= 0 && candidate >= openStart))
				{
					continue;
				}

				openStart = candidate;
				openEnd = candidate + tagName.Length + 2;
			}

			if (openStart < 0)
			{
				stripped.Append(content, cursor, content.Length - cursor);

				break;
			}

			stripped.Append(content, cursor, openStart - cursor);

			// The tag name is whatever sits between the angle brackets we matched.
			string openedTag = content[(openStart + 1)..(openEnd - 1)];
			int closeStart = content.IndexOf(
				$"</{openedTag}>",
				openEnd,
				StringComparison.OrdinalIgnoreCase);

			if (closeStart < 0)
			{
				if (openStart == 0)
				{
					// Truncated mid-thought — drop the remainder.
					break;
				}

				// Not a reasoning block after all; keep it as the literal text it is.
				stripped.Append(content, openStart, content.Length - openStart);

				break;
			}

			cursor = closeStart + openedTag.Length + 3;
		}

		return stripped.ToString().Trim();
	}
}

internal sealed record OpenAICompatibleRequest(
	[property: JsonPropertyName("model")] string Model,
	[property: JsonPropertyName("messages")]
	IReadOnlyList<OpenAICompatibleMessage> Messages,
	[property: JsonPropertyName("max_tokens")]
	int MaxTokens,
	[property: JsonPropertyName("temperature")]
	double Temperature)
{
#pragma warning disable CA1822
	// Must stay an instance member: System.Text.Json does not serialize static
	// properties, and making this static silently drops "stream": false from the
	// request body.
	[JsonPropertyName("stream")]
	public bool Stream => false;
#pragma warning restore CA1822
}

// Content is a string, or an OpenAIContentPart[] when images are attached.
internal sealed record OpenAICompatibleMessage(
	[property: JsonPropertyName("role")] string Role,
	[property: JsonPropertyName("content")]
	object Content);

internal sealed record OpenAICompatibleResponse(
	[property: JsonPropertyName("choices")]
	IReadOnlyList<OpenAICompatibleChoice>? Choices);

internal sealed record OpenAICompatibleChoice(
	[property: JsonPropertyName("message")]
	OpenAICompatibleChoiceMessage? Message);

internal sealed record OpenAICompatibleChoiceMessage(
	[property: JsonPropertyName("content")]
	string? Content);
