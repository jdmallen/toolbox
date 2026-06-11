using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Structs;

/// <summary>
/// A compact, URL-friendly representation of a GUID using Base64 encoding.
/// A customized and improved version of Dave Transom's ShortGuid.
/// See
/// http://www.singular.co.nz/2007/12/shortguid-a-shorter-and-url-friendly-guid-in-c-sharp/
/// </summary>
/// <remarks>
/// MiniGuid encodes a standard GUID into a 22-character Base64 string,
/// making it more suitable for URLs and APIs while maintaining uniqueness.
/// </remarks>
[JsonConverter(typeof(MiniGuidJsonConverter))]

// ReSharper disable once StructCanBeMadeReadOnly
public struct MiniGuid
	: IComparable,
		IComparable<MiniGuid>,
		IComparable<Guid>,
		IEquatable<MiniGuid>,
		IEquatable<Guid>
{
	// Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="MiniGuid" /> struct from an
	/// encoded string representation.
	/// </summary>
	/// <param name="value">The Base64-encoded MiniGuid string.</param>
	public MiniGuid(string value)
	{
		Guid = Decode(value);
		Value = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="MiniGuid" /> struct from a
	/// standard GUID.
	/// </summary>
	/// <param name="value">The GUID to encode as a MiniGuid.</param>
	public MiniGuid(Guid value)
	{
		Guid = value;
		Value = Encode(value);
	}

	// Properties

	/// <summary>
	/// Gets the underlying GUID value.
	/// </summary>
	[SuppressMessage(
		"Naming",
		"CA1720:Identifier contains type name",
		Justification =
			"\"Guid\" is the canonical name for the wrapped System.Guid; renaming "
			+ "would break the public API and reduce clarity.")]
	public Guid Guid { get; }

	/// <summary>
	/// Gets the Base64-encoded string representation of the MiniGuid.
	/// </summary>
	public string Value { get; }

	// Encode / Decode

	/// <summary>
	/// Creates a new MiniGuid with a randomly generated GUID value.
	/// </summary>
	/// <returns>A new MiniGuid with a unique GUID.</returns>
	public static MiniGuid NewGuid() => new(Guid.NewGuid());

	/// <summary>
	/// Encodes a GUID string into a MiniGuid string representation.
	/// </summary>
	/// <param name="value">The GUID string to encode.</param>
	/// <returns>The Base64-encoded MiniGuid string.</returns>
	public static string Encode(string value) => Encode(new Guid(value));

	/// <summary>
	/// Encodes a GUID into its MiniGuid string representation.
	/// </summary>
	/// <param name="value">The GUID to encode.</param>
	/// <returns>A 22-character Base64-encoded string representation of the GUID.</returns>
	public static string Encode(Guid value) => Convert.ToBase64String(value.ToByteArray())
		.Replace('/', '_')
		.Replace('+', '-')

		// ReSharper disable once ReplaceSubstringWithRangeIndexer
		.Substring(0, 22);

	/// <summary>
	/// Decodes a MiniGuid string back into a standard GUID.
	/// </summary>
	/// <param name="value">The Base64-encoded MiniGuid string to decode.</param>
	/// <returns>
	/// The decoded GUID, or <see cref="Guid.Empty" /> if the value is null or
	/// whitespace.
	/// </returns>
	public static Guid Decode(string value) => string.IsNullOrWhiteSpace(value)
		? Guid.Empty
		: new Guid(
			Convert.FromBase64String(
				value.Replace('_', '/')
					.Replace('-', '+')
				+ "=="));

	// Implicit Operators

	/// <summary>
	/// Implicitly converts a MiniGuid to its string representation.
	/// </summary>
	/// <param name="miniGuid">The MiniGuid to convert.</param>
	/// <returns>The Base64-encoded string value.</returns>
	public static implicit operator string(MiniGuid miniGuid) => miniGuid.Value;

	/// <summary>
	/// Implicitly converts a MiniGuid to its underlying GUID value.
	/// </summary>
	/// <param name="miniGuid">The MiniGuid to convert.</param>
	/// <returns>The underlying GUID.</returns>
	public static implicit operator Guid(MiniGuid miniGuid) => miniGuid.Guid;

	/// <summary>
	/// Implicitly converts a string to a MiniGuid.
	/// </summary>
	/// <param name="miniGuid">The Base64-encoded MiniGuid string.</param>
	/// <returns>A new MiniGuid instance.</returns>
	public static implicit operator MiniGuid(string miniGuid) => new(miniGuid);

	/// <summary>
	/// Implicitly converts a GUID to a MiniGuid.
	/// </summary>
	/// <param name="value">The GUID to convert.</param>
	/// <returns>A new MiniGuid instance.</returns>
	public static implicit operator MiniGuid(Guid value) => new(value);

	// Comparison Operations

	/// <summary>
	/// Determines whether two MiniGuid values are equal.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>true if the underlying GUIDs are equal; otherwise, false.</returns>
	public static bool operator ==(MiniGuid x, MiniGuid y) => x.Guid == y.Guid;

	/// <summary>
	/// Determines whether two MiniGuid values are not equal.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>true if the underlying GUIDs are not equal; otherwise, false.</returns>
	public static bool operator !=(MiniGuid x, MiniGuid y) => !(x == y);

	/// <summary>
	/// Determines whether one MiniGuid sorts before another.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>
	/// true if <paramref name="x" /> sorts before <paramref name="y" />;
	/// otherwise, false.
	/// </returns>
	public static bool operator <(MiniGuid x, MiniGuid y) => x.CompareTo(y) < 0;

	/// <summary>
	/// Determines whether one MiniGuid sorts before or equal to another.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>
	/// true if <paramref name="x" /> does not sort after
	/// <paramref name="y" />; otherwise, false.
	/// </returns>
	public static bool operator <=(MiniGuid x, MiniGuid y) => x.CompareTo(y) <= 0;

	/// <summary>
	/// Determines whether one MiniGuid sorts after another.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>
	/// true if <paramref name="x" /> sorts after <paramref name="y" />;
	/// otherwise, false.
	/// </returns>
	public static bool operator >(MiniGuid x, MiniGuid y) => x.CompareTo(y) > 0;

	/// <summary>
	/// Determines whether one MiniGuid sorts after or equal to another.
	/// </summary>
	/// <param name="x">The first MiniGuid to compare.</param>
	/// <param name="y">The second MiniGuid to compare.</param>
	/// <returns>
	/// true if <paramref name="x" /> does not sort before
	/// <paramref name="y" />; otherwise, false.
	/// </returns>
	public static bool operator >=(MiniGuid x, MiniGuid y) => x.CompareTo(y) >= 0;

	/// <summary>
	/// Compares this MiniGuid to another object.
	/// </summary>
	/// <param name="obj">The object to compare to.</param>
	/// <returns>
	/// A signed integer indicating the relative values of this instance and
	/// obj.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Thrown if obj is not a MiniGuid, Guid, or
	/// string.
	/// </exception>
	public int CompareTo(object? obj)
	{
		return obj switch
		{
			null              => 1,
			Guid guid         => CompareTo(guid),
			MiniGuid miniGuid => CompareTo(miniGuid),
			string str        => string.Compare(Value, str, StringComparison.Ordinal),
			_                 => throw new ArgumentException("Object is not a MiniGuid", nameof(obj)),
		};
	}

	/// <summary>
	/// Compares this MiniGuid to another MiniGuid using ordinal string comparison.
	/// </summary>
	/// <param name="other">The other MiniGuid to compare to.</param>
	/// <returns>A signed integer indicating the relative values.</returns>
	public int CompareTo(MiniGuid other)
		=> string.Compare(Value, other.Value, StringComparison.Ordinal);

	/// <summary>
	/// Compares the underlying GUID of this MiniGuid to another GUID.
	/// </summary>
	/// <param name="other">The GUID to compare to.</param>
	/// <returns>A signed integer indicating the relative values.</returns>
	public int CompareTo(Guid other) => Guid.CompareTo(other);

	/// <summary>
	/// Determines whether this MiniGuid is equal to another MiniGuid.
	/// </summary>
	/// <param name="other">The other MiniGuid to compare to.</param>
	/// <returns>true if the underlying GUIDs are equal; otherwise, false.</returns>
	public bool Equals(MiniGuid other) => Guid.Equals(other.Guid);

	/// <summary>
	/// Determines whether this MiniGuid is equal to a GUID.
	/// </summary>
	/// <param name="other">The GUID to compare to.</param>
	/// <returns>true if the underlying GUIDs are equal; otherwise, false.</returns>
	public bool Equals(Guid other) => Guid.Equals(other);

	// Object Overrides

	/// <summary>
	/// Determines whether this MiniGuid is equal to the specified object.
	/// </summary>
	/// <param name="obj">The object to compare to.</param>
	/// <returns>
	/// true if obj is a MiniGuid, Guid, or string that represents an equal
	/// value; otherwise, false.
	/// </returns>
	public override bool Equals(object? obj)
	{
		return obj switch
		{
			Guid guid         => Equals(guid),
			MiniGuid miniGuid => Equals(miniGuid),
			string str        => Guid.Equals(((MiniGuid)str).Guid),
			_                 => false,
		};
	}

	/// <summary>
	/// Returns the hash code for this MiniGuid based on its underlying GUID.
	/// </summary>
	/// <returns>The hash code of the underlying GUID.</returns>
	public readonly override int GetHashCode() => Guid.GetHashCode();

	/// <summary>
	/// Returns the Base64-encoded string representation of this MiniGuid.
	/// </summary>
	/// <returns>The Base64-encoded string value.</returns>
	public readonly override string ToString() => Value;
}

/// <summary>
/// JSON converter for serializing and deserializing MiniGuid values.
/// Handles conversion between MiniGuid and its string representation in JSON.
/// </summary>
[UsedImplicitly]
public class MiniGuidJsonConverter : JsonConverter
{
	/// <summary>
	/// Determines whether the converter can convert the specified type.
	/// </summary>
	/// <param name="objectType">The type to check.</param>
	/// <returns>true if the type is MiniGuid or MiniGuid?; otherwise, false.</returns>
	public override bool CanConvert(Type objectType)
		=> objectType == typeof(MiniGuid) || objectType == typeof(MiniGuid?);

	/// <summary>
	/// Reads a MiniGuid value from JSON.
	/// </summary>
	/// <param name="reader">The JSON reader.</param>
	/// <param name="objectType">The type of the object to deserialize.</param>
	/// <param name="existingValue">The existing value (not used).</param>
	/// <param name="serializer">The JSON serializer.</param>
	/// <returns>The deserialized MiniGuid value.</returns>
	public override object ReadJson(
		JsonReader reader,
		Type objectType,
		object? existingValue,
		JsonSerializer serializer)
		=> ((MiniGuid)reader.Value?.ToString()! == null!
			? new JsonSerializer().Deserialize(reader, objectType)
			: (MiniGuid)reader.Value?.ToString()!)!;

	/// <summary>
	/// Writes a MiniGuid value to JSON.
	/// </summary>
	/// <param name="writer">The JSON writer.</param>
	/// <param name="value">The MiniGuid value to serialize.</param>
	/// <param name="serializer">The JSON serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, (MiniGuid)value! == null! ? value : ((MiniGuid)value).Value);
	}
}
