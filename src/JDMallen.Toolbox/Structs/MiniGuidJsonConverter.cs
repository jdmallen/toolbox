using System.Text.Json;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.Structs;

/// <summary>
/// System.Text.Json converter that reads and writes a <see cref="MiniGuid" />
/// as its 22-character string, both as a value and as a dictionary key.
/// <see cref="MiniGuid" /> applies it by attribute, so no registration is
/// needed. <c>MiniGuid?</c> is handled too, with JSON <c>null</c> for null.
/// </summary>
public sealed class MiniGuidJsonConverter : JsonConverter<MiniGuid>
{
	private const int ENCODED_LENGTH = 22;

	// Encoding from the Guid (rather than writing Value) means default(MiniGuid),
	// whose Value is null, still writes the 22-character empty-Guid encoding.
	private static string Format(MiniGuid value) => MiniGuid.Encode(value.Guid);

	private static MiniGuid Parse(string? value)
	{
		if (value is null || value.Length != ENCODED_LENGTH)
		{
			throw new JsonException(
				$"A {nameof(MiniGuid)} must be a {ENCODED_LENGTH}-character string.");
		}

		try
		{
			return new MiniGuid(value);
		}
		catch (FormatException exception)
		{
			throw new JsonException(
				$"'{value}' is not a valid {nameof(MiniGuid)}.",
				exception);
		}
	}

	/// <inheritdoc />
	public override MiniGuid Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.String)
		{
			throw new JsonException(
				$"Expected a string for {nameof(MiniGuid)}, found {reader.TokenType}.");
		}

		return Parse(reader.GetString());
	}

	/// <inheritdoc />
	public override MiniGuid ReadAsPropertyName(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
		=> Parse(reader.GetString());

	/// <inheritdoc />
	public override void Write(
		Utf8JsonWriter writer,
		MiniGuid value,
		JsonSerializerOptions options)
		=> writer.WriteStringValue(Format(value));

	/// <inheritdoc />
	public override void WriteAsPropertyName(
		Utf8JsonWriter writer,
		MiniGuid value,
		JsonSerializerOptions options)
		=> writer.WritePropertyName(Format(value));
}
