namespace JDMallen.Toolbox.AspNetCore.Options;

/// <summary>
/// Configuration options for password complexity validation.
/// </summary>
public class PasswordComplexityOptions
{
	/// <summary>
	/// Gets or sets the minimum bits of entropy threshold required for a password.
	/// Defaults to 50 if not set or set to zero or negative.
	/// </summary>
	public float BitsThreshold
	{
		get => field <= 0 ? 50F : field;
		set;
	}

	/// <summary>
	/// Gets or sets the minimum password length.
	/// Defaults to 8 if not set or set to less than 2.
	/// </summary>
	public int MinimumLength
	{
		get => field < 2 ? 8 : field;
		set;
	}
}
