using JDMallen.Toolbox.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JDMallen.Toolbox.Models
{
	public class PasswordResult
	{
		public float BitsOfEntropy { get; set; }

		public int Length { get; set; }

		public PasswordStrength Strength { get; set; }

		public bool IsError => Error != PasswordError.None;

		[JsonConverter(typeof(StringEnumConverter))]
		public PasswordError Error { get; set; } = PasswordError.None;
	}
}
