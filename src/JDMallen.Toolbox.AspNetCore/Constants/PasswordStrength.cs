namespace JDMallen.Toolbox.AspNetCore.Constants;

/// <summary>
/// Represents the strength level of a password based on entropy calculation.
/// </summary>
public enum PasswordStrength
{
	/// <summary>
	/// Password has poor strength (0-30 bits of entropy).
	/// </summary>
	Poor,

	/// <summary>
	/// Password has fair strength (30-60 bits of entropy).
	/// </summary>
	Fair,

	/// <summary>
	/// Password has good strength (60-90 bits of entropy).
	/// </summary>
	Good,

	/// <summary>
	/// Password has great strength (90-120 bits of entropy).
	/// </summary>
	Great,

	/// <summary>
	/// Password has excellent strength (over 120 bits of entropy).
	/// </summary>
	Excellent
}
