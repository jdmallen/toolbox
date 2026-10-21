using System.Text.Json;
using JDMallen.Toolbox.Structs;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Characterization tests for <see cref="MiniGuid" />: they pin the current
/// encoding, equality, and ordering, and cover the System.Text.Json converter
/// that replaced the Newtonsoft one in 4.0.
/// </summary>
public class MiniGuidTests
{
	// Expected encodings were computed independently (Python's uuid.bytes_le has
	// the same byte order as Guid.ToByteArray). The second vector produces both
	// '/' and '+' in raw Base64, so it covers the URL-safe substitutions.
	private static readonly Guid PlainGuid =
		new("0f8fad5b-d9cb-469f-a165-70867728950e");

	private const string PlainEncoded = "W62PD8vZn0ahZXCGdyiVDg";

	private static readonly Guid SubstitutedGuid =
		new("fbffffff-ffff-ffff-ffff-ffffffffffff");

	private const string SubstitutedEncoded = "____-________________w";

	[Theory]
	[InlineData("0f8fad5b-d9cb-469f-a165-70867728950e", PlainEncoded)]
	[InlineData("fbffffff-ffff-ffff-ffff-ffffffffffff", SubstitutedEncoded)]
	[InlineData("00000000-0000-0000-0000-000000000000", "AAAAAAAAAAAAAAAAAAAAAA")]
	public void Encode_ProducesKnownUrlSafeValue(string guid, string expected)
	{
		Assert.Equal(expected, MiniGuid.Encode(new Guid(guid)));
		Assert.Equal(expected, MiniGuid.Encode(guid));
	}

	[Theory]
	[InlineData(PlainEncoded, "0f8fad5b-d9cb-469f-a165-70867728950e")]
	[InlineData(SubstitutedEncoded, "fbffffff-ffff-ffff-ffff-ffffffffffff")]
	public void Decode_ReversesEncode(string encoded, string expected)
	{
		Assert.Equal(new Guid(expected), MiniGuid.Decode(encoded));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Decode_ReturnsEmptyForBlankInput(string? value)
	{
		Assert.Equal(Guid.Empty, MiniGuid.Decode(value!));
	}

	[Fact]
	public void BlankStringConstructor_KeepsTheOriginalValue()
	{
		// The string constructor stores its input verbatim rather than
		// re-encoding the decoded (empty) Guid.
		var miniGuid = new MiniGuid("");

		Assert.Equal(Guid.Empty, miniGuid.Guid);
		Assert.Equal("", miniGuid.Value);
	}

	[Fact]
	public void NewGuid_RoundTripsThroughBothConstructors()
	{
		MiniGuid original = MiniGuid.NewGuid();

		Assert.Equal(22, original.Value.Length);
		Assert.Equal(original.Guid, new MiniGuid(original.Value).Guid);
		Assert.Equal(original.Value, new MiniGuid(original.Guid).Value);
		Assert.Equal(original.Value, original.ToString());
	}

	[Fact]
	public void ImplicitConversions_PreserveTheValue()
	{
		MiniGuid fromGuid = PlainGuid;
		MiniGuid fromString = PlainEncoded;
		Guid asGuid = fromString;
		string asString = fromGuid;

		Assert.Equal(PlainGuid, asGuid);
		Assert.Equal(PlainEncoded, asString);
	}

	[Fact]
	public void Equality_ComparesTheUnderlyingGuid()
	{
		var miniGuid = new MiniGuid(PlainGuid);
		var same = new MiniGuid(PlainEncoded);
		var other = new MiniGuid(SubstitutedGuid);

		Assert.True(miniGuid == same);
		Assert.False(miniGuid != same);
		Assert.True(miniGuid != other);
		Assert.True(miniGuid.Equals(same));
		Assert.True(miniGuid.Equals(PlainGuid));
		Assert.Equal(miniGuid.GetHashCode(), PlainGuid.GetHashCode());
	}

	[Fact]
	public void EqualsObject_AcceptsMiniGuidGuidAndString()
	{
		var miniGuid = new MiniGuid(PlainGuid);

		Assert.True(miniGuid.Equals((object)new MiniGuid(PlainGuid)));
		Assert.True(miniGuid.Equals((object)PlainGuid));
		Assert.True(miniGuid.Equals((object)PlainEncoded));
		Assert.False(miniGuid.Equals((object)SubstitutedEncoded));
		Assert.False(miniGuid.Equals((object?)null));
		Assert.False(miniGuid.Equals(42));
	}

	[Fact]
	public void Ordering_DependsOnWhichOverloadIsUsed()
	{
		// Pinned as-is: comparing two MiniGuids is ordinal on the encoded
		// string, while comparing against a Guid uses Guid ordering. These two
		// Guids sort in opposite orders under the two rules.
		var lowGuid = new Guid("00000001-0000-0000-0000-000000000000");
		var highGuid = new Guid("00000100-0000-0000-0000-000000000000");
		var low = new MiniGuid(lowGuid);
		var high = new MiniGuid(highGuid);

		Assert.True(low.CompareTo(highGuid) < 0);
		Assert.True(low.CompareTo(high) > 0);
		Assert.True(low > high);
		Assert.True(low >= high);
		Assert.False(low < high);
		Assert.False(low <= high);
		Assert.True(low.CompareTo((object)highGuid) < 0);
		Assert.True(low.CompareTo((object)high) > 0);
		Assert.True(low.CompareTo((object)high.Value) > 0);
	}

	[Fact]
	public void CompareToObject_HandlesNullAndRejectsOtherTypes()
	{
		var miniGuid = new MiniGuid(PlainGuid);

		Assert.Equal(1, miniGuid.CompareTo((object?)null));
		Assert.Equal(0, miniGuid.CompareTo((object)PlainEncoded));
		Assert.Throws<ArgumentException>(() => miniGuid.CompareTo(42));
	}

	private sealed class Holder
	{
		public MiniGuid Id { get; set; }

		public MiniGuid? OptionalId { get; set; }
	}

	[Fact]
	public void Json_SerializesAsTheEncodedString()
	{
		// Same shape the 3.x Newtonsoft converter wrote.
		string json = JsonSerializer.Serialize(
			new Holder { Id = new MiniGuid(PlainGuid) });

		Assert.Equal($"{{\"Id\":\"{PlainEncoded}\",\"OptionalId\":null}}", json);
	}

	[Fact]
	public void Json_RoundTripsValueAndNullable()
	{
		var original = new Holder
		{
			Id = new MiniGuid(SubstitutedGuid),
			OptionalId = new MiniGuid(PlainGuid),
		};

		Holder copy = JsonSerializer.Deserialize<Holder>(
			JsonSerializer.Serialize(original))!;

		Assert.Equal(SubstitutedGuid, copy.Id.Guid);
		Assert.Equal(PlainGuid, copy.OptionalId!.Value.Guid);
	}

	[Fact]
	public void Json_WritesDefaultAsTheEmptyGuidEncoding()
	{
		// default(MiniGuid) has a null Value; the converter encodes the Guid
		// instead, so it never writes JSON null for a non-nullable MiniGuid.
		Assert.Equal(
			"\"AAAAAAAAAAAAAAAAAAAAAA\"",
			JsonSerializer.Serialize(default(MiniGuid)));
	}

	[Fact]
	public void Json_WorksAsADictionaryKey()
	{
		var map = new Dictionary<MiniGuid, int> { [new MiniGuid(PlainGuid)] = 1 };

		string json = JsonSerializer.Serialize(map);

		Assert.Equal($"{{\"{PlainEncoded}\":1}}", json);
		Assert.Equal(
			1,
			JsonSerializer.Deserialize<Dictionary<MiniGuid, int>>(json)![
				new MiniGuid(PlainGuid)]);
	}

	[Theory]
	[InlineData("123")]
	[InlineData("\"\"")]
	[InlineData("\"tooShort\"")]
	[InlineData("\"!!!!!!!!!!!!!!!!!!!!!!\"")]
	public void Json_RejectsInvalidInput(string json)
	{
		Assert.Throws<JsonException>(
			() => JsonSerializer.Deserialize<MiniGuid>(json));
	}
}
