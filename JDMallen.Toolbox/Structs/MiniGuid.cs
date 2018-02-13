using System;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Structs
{
	/// <summary>
	/// A customized (and in some ways, improved) version of Dave Transom's ShortGuid:
	/// http://www.singular.co.nz/2007/12/shortguid-a-shorter-and-url-friendly-guid-in-c-sharp/ 
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

		public static MiniGuid NewGuid() => new MiniGuid(Guid.NewGuid());

		public static string Encode(string value) => Encode(new Guid(value));

		public static string Encode(Guid guid)
			=> Convert.ToBase64String(guid.ToByteArray())
			          .Replace("/", "_")
			          .Replace("+", "-")
			          .Substring(0, 22);

		public static Guid Decode(string value)
			=> string.IsNullOrWhiteSpace(value)
				   ? Guid.Empty
				   : new Guid(Convert.FromBase64String(value.Replace("_", "/")
				                                            .Replace("-", "+")
				                                       + "=="));

		#endregion

		#region Implicit Operators

		public static implicit operator string(MiniGuid miniGuid) => miniGuid.Value;

		public static implicit operator Guid(MiniGuid miniGuid) => miniGuid.Guid;

		public static implicit operator MiniGuid(string miniGuid) => new MiniGuid(miniGuid);

		public static implicit operator MiniGuid(Guid guid) => new MiniGuid(guid);

		#endregion

		#region Comparison Operations

		public static bool operator ==(MiniGuid x, MiniGuid y) => x.Guid == y.Guid;

		public static bool operator !=(MiniGuid x, MiniGuid y) => !(x == y);

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
			=> string.Compare(Value, other.Value, StringComparison.Ordinal);

		public int CompareTo(Guid other) => Guid.CompareTo(other);

		public bool Equals(MiniGuid other) => Guid.Equals(other.Guid);

		public bool Equals(Guid other) => Guid.Equals(other);

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
					return Guid.Equals(((MiniGuid) str).Guid);
				default:
					return false;
			}
		}

		public override int GetHashCode() => Guid.GetHashCode();

		public override string ToString() => Value;

		#endregion

	}

	public class MiniGuidJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			=> serializer.Serialize(writer, (MiniGuid) value == null ? value : ((MiniGuid) value).Value);

		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
			=> (MiniGuid) reader.Value?.ToString() == null
				   ? new JsonSerializer().Deserialize(reader, objectType)
				   : (MiniGuid) reader.Value?.ToString();

		public override bool CanConvert(Type objectType)
			=> objectType == typeof(MiniGuid) || objectType == typeof(MiniGuid?);
	}
}
