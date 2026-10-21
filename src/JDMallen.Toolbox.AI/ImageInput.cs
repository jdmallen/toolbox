namespace JDMallen.Toolbox.AI;

/// <summary>
/// An image attached to a <see cref="CompletionRequest" />, sent alongside the
/// user prompt to vision-capable models.
/// </summary>
/// <param name="Data">The raw encoded image bytes (not base64).</param>
/// <param name="MediaType">
/// The image's MIME type, e.g. "image/jpeg" or "image/png". Supported types vary
/// by provider; Anthropic accepts jpeg, png, gif, and webp.
/// </param>
public sealed record ImageInput(ReadOnlyMemory<byte> Data, string MediaType)
{
	internal string ToBase64() => Convert.ToBase64String(Data.Span);

	internal string ToDataUri() => $"data:{MediaType};base64,{ToBase64()}";
}
