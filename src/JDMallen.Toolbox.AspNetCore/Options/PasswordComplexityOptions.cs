namespace JDMallen.Toolbox.AspNetCore.Options;

/// <summary>
/// Configuration options for password complexity validation.
/// </summary>
public class PasswordComplexityOptions
{
	private float _bitsThreshold;
	private int _minimumLength;

	/// <summary>
	/// Gets or sets the minimum bits of entropy threshold required for a password.
	/// Defaults to 50 if not set or set to zero or negative.
	/// </summary>
	public float BitsThreshold
	{
		get => _bitsThreshold <= 0 ? 50F : _bitsThreshold;
		set => _bitsThreshold = value;
	}

	/// <summary>
	/// Gets or sets the minimum password length.
	/// Defaults to 8 if not set or set to less than 2.
	/// </summary>
	public int MinimumLength
	{
		get => _minimumLength < 2 ? 8 : _minimumLength;
		set => _minimumLength = value;
	}
}
