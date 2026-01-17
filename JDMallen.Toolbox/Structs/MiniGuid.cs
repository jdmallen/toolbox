using System;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Structs;

/// <summary>
///   A customized (and in some ways, improved) version of Dave Transom's ShortGuid:
///   http://www.singular.co.nz/2007/12/shortguid-a-shorter-and-url-friendly-guid-in-c-sharp/
/// </summary>
[JsonConverter(typeof(MiniGuidJsonConverter))]
// ReSharper disable once InheritdocConsiderUsage
public struct MiniGuid
	: IComparable,
		IComparable<MiniGuid>,
		IComparable<Guid>,
		IEquatable<MiniGuid>,
		IEquatable<Guid>
{
	#region Constructors

	public MiniGuid(string value)
	{
		Guid = Decode(value);
		Value = value;
	}

	public MiniGuid(Guid value)
	{
		Guid = value;
		Value = Encode(value);
	}

	#endregion

	#region Properties

	public Guid Guid { get; }

	public string Value { get; }

	#endregion

	#region Encode / Decode

	public static MiniGuid NewGuid()
	{
		return new MiniGuid(Guid.NewGuid());
	}

	public static string Encode(string value)
	{
		return Encode(new Guid(value));
	}

	public static string Encode(Guid guid)
	{
		return Convert.ToBase64String(guid.ToByteArray())
			.Replace("/", "_")
			.Replace("+", "-")
			.Substring(0, 22);
	}

	public static Guid Decode(string value)
	{
		return string.IsNullOrWhiteSpace(value)
			? Guid.Empty
			: new Guid(Convert.FromBase64String(value.Replace("_", "/")
				                                    .Replace("-", "+")
			                                    + "=="));
	}

	#endregion

	#region Implicit Operators

	public static implicit operator string(MiniGuid miniGuid)
	{
		return miniGuid.Value;
	}

	public static implicit operator Guid(MiniGuid miniGuid)
	{
		return miniGuid.Guid;
	}

	public static implicit operator MiniGuid(string miniGuid)
	{
		return new MiniGuid(miniGuid);
	}

	public static implicit operator MiniGuid(Guid guid)
	{
		return new MiniGuid(guid);
	}

	#endregion

	#region Comparison Operations

	public static bool operator ==(MiniGuid x, MiniGuid y)
	{
		return x.Guid == y.Guid;
	}

	public static bool operator !=(MiniGuid x, MiniGuid y)
	{
		return !(x == y);
	}

	public int CompareTo(object obj)
	{
		switch (obj)
		{
			case null:
				return 1;
			case Guid guid:
				return CompareTo(guid);
			case MiniGuid miniGuid:
				return CompareTo(miniGuid);
			case string str:
				return string.Compare(Value, str, StringComparison.Ordinal);
			default:
				throw new ArgumentException("Object is not a MiniGuid", nameof(obj));
		}
	}

	public int CompareTo(MiniGuid other)
	{
		return string.Compare(Value, other.Value, StringComparison.Ordinal);
	}

	public int CompareTo(Guid other)
	{
		return Guid.CompareTo(other);
	}

	public bool Equals(MiniGuid other)
	{
		return Guid.Equals(other.Guid);
	}

	public bool Equals(Guid other)
	{
		return Guid.Equals(other);
	}

	#endregion

	#region Object Overrides

	public override bool Equals(object obj)
	{
		switch (obj)
		{
			case Guid guid:
				return Equals(guid);
			case MiniGuid miniGuid:
				return Equals(miniGuid);
			case string str:
				return Guid.Equals(((MiniGuid)str).Guid);
			default:
				return false;
		}
	}

	public override int GetHashCode()
	{
		return Guid.GetHashCode();
	}

	public override string ToString()
	{
		return Value;
	}

	#endregion
}

public class MiniGuidJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, (MiniGuid)value == null ? value : ((MiniGuid)value).Value);
	}

	public override object ReadJson(
		JsonReader reader,
		Type objectType,
		object existingValue,
		JsonSerializer serializer)
	{
		return (MiniGuid)reader.Value?.ToString() == null
			? new JsonSerializer().Deserialize(reader, objectType)
			: (MiniGuid)reader.Value?.ToString();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(MiniGuid) || objectType == typeof(MiniGuid?);
	}
}