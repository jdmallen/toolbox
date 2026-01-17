namespace JDMallen.Toolbox.AspNetCore.Constants;

/// <summary>
/// Flags enumeration representing password validation errors.
/// </summary>
[Flags]
public enum PasswordError
{
	/// <summary>
	/// No password errors detected.
	/// </summary>
	None = 0,

	/// <summary>
	/// Password does not meet the minimum length requirement.
	/// </summary>
	TooShort = 1,

	/// <summary>
	/// Password is found in the common passwords list.
	/// </summary>
	TooCommon = 2,

	/// <summary>
	/// Password does not meet the minimum complexity threshold (bits of entropy).
	/// </summary>
	NotComplexEnough = 4
}
