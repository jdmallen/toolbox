using JDMallen.Toolbox.Structs;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Characterization tests for <see cref="MiniGuid" />: they pin the current
/// encoding, equality, and ordering so the 4.0 changes (a System.Text.Json
/// converter in place of the Newtonsoft one) can't silently alter them.
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
	}

	[Fact]
	public void NewtonsoftJson_SerializesAsTheEncodedString()
	{
		// Phase 2 replaces the Newtonsoft converter with a System.Text.Json one;
		// it must produce this same JSON shape.
		string json = JsonConvert.SerializeObject(
			new Holder { Id = new MiniGuid(PlainGuid) });

		Assert.Equal($"{{\"Id\":\"{PlainEncoded}\"}}", json);
		Assert.Equal(
			PlainGuid,
			JsonConvert.DeserializeObject<Holder>(json)!.Id.Guid);
	}
}
