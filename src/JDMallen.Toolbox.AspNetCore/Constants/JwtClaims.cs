namespace JDMallen.Toolbox.AspNetCore.Constants;

/// <summary>
/// Constants for custom JWT claim values used in application authorization.
/// </summary>
public class JwtClaims
{
	/// <summary>
	/// Claim value for standard API user role.
	/// </summary>
	public const string ApiUser = "ApiUser";

	/// <summary>
	/// Claim value for API manager role with elevated permissions.
	/// </summary>
	public const string ApiManager = "ApiManager";
}
