namespace JDMallen.Toolbox.AspNetCore.Constants;

/// <summary>
/// Additional JWT claim type constants not covered by standard claims.
/// </summary>
public static class OtherJwtClaimTypes
{
	/// <summary>
	/// The primary security identifier claim type.
	/// </summary>
	public const string PRIMARY_SID = "sid";

	/// <summary>
	/// The email address claim type.
	/// </summary>
	public const string EMAIL = "email";
}
