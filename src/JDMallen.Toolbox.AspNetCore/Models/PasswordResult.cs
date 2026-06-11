using JDMallen.Toolbox.AspNetCore.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JDMallen.Toolbox.AspNetCore.Models;

/// <summary>
/// Represents the result of a password complexity analysis.
/// </summary>
public class PasswordResult
{
	/// <summary>
	/// Gets or sets the calculated bits of entropy for the password.
	/// Higher values indicate greater password complexity.
	/// </summary>
	public float BitsOfEntropy { get; set; }

	/// <summary>
	/// Gets or sets any validation errors associated with the password.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public PasswordError Error { get; set; } = PasswordError.None;

	/// <summary>
	/// Gets a value indicating whether the password validation resulted in an error.
	/// </summary>
	public bool IsError => Error != PasswordError.None;

	/// <summary>
	/// Gets or sets the length of the password.
	/// </summary>
	public int Length { get; set; }

	/// <summary>
	/// Gets or sets the overall strength rating of the password.
	/// </summary>
	public PasswordStrength Strength { get; set; }
}
