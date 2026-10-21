using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Builds the user message <c>content</c> for the OpenAI chat completions shape,
/// shared by <see cref="OpenAICompatibleChatClient" /> and
/// <see cref="AzureOpenAIChatClient" />.
/// </summary>
internal static class OpenAIContentParts
{
	/// <summary>
	/// Returns the prompt as a plain string when there are no images, so
	/// text-only requests keep the original wire format (some self-hosted runners
	/// reject the array form outright). With images, returns an array of parts:
	/// each image as an <c>image_url</c> data URI, then the prompt text.
	/// </summary>
	/// <remarks>
	/// Typed as <see cref="object" /> because the field is either a string or an
	/// array; both runtime types are registered in <see cref="AIJsonContext" />,
	/// and an explicit array (not a collection expression) keeps the runtime type
	/// one the context knows.
	/// </remarks>
	public static object Build(CompletionRequest request)
	{
		if (request.Images.Count == 0)
		{
			return request.UserPrompt;
		}

		var parts = new OpenAIContentPart[request.Images.Count + 1];

		for (var index = 0; index < request.Images.Count; index++)
		{
			parts[index] = new OpenAIContentPart("image_url")
			{
				ImageUrl = new OpenAIImageUrl(request.Images[index].ToDataUri()),
			};
		}

		parts[^1] = new OpenAIContentPart("text") { Text = request.UserPrompt };

		return parts;
	}
}

internal sealed record OpenAIContentPart(
	[property: JsonPropertyName("type")] string Type)
{
	[JsonPropertyName("text")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? Text { get; init; }

	[JsonPropertyName("image_url")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public OpenAIImageUrl? ImageUrl { get; init; }
}

internal sealed record OpenAIImageUrl(
	[property: JsonPropertyName("url")] string Url);
