namespace JDMallen.Toolbox.AspNetCore.Constants;

/// <summary>
/// Additional JWT claim type constants not covered by standard claims.
/// </summary>
public class OtherJwtClaimTypes
{
	/// <summary>
	/// The primary security identifier claim type.
	/// </summary>
	public const string PrimarySid = "sid";

	/// <summary>
	/// The email address claim type.
	/// </summary>
	public const string Email = "email";
}
