using System.Security.Claims;

namespace JDMallen.Toolbox.AspNetCore.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal objects.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Gets the user ID as a Guid from the ClaimsPrincipal.
	/// </summary>
	public static Guid GetUserIdAsGuid(this ClaimsPrincipal principal)
	{
		var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
		            ?? principal.FindFirst("userId");

		if (claim is null || !Guid.TryParse(claim.Value, out var userId))
		{
			throw new InvalidOperationException(
				"User ID claim not found or invalid.");
		}

		return userId;
	}

	/// <summary>
	/// Gets the user ID as a string from the ClaimsPrincipal.
	/// </summary>
	public static string GetUserId(this ClaimsPrincipal principal)
	{
		return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
		       ?? principal.FindFirst("userId")?.Value
		       ?? throw new InvalidOperationException("User ID claim not found.");
	}

	/// <summary>
	/// Tries to get the user ID as a Guid from the ClaimsPrincipal.
	/// </summary>
	public static bool TryGetUserIdAsGuid(
		this ClaimsPrincipal principal,
		out Guid userId)
	{
		userId = Guid.Empty;
		var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
		            ?? principal.FindFirst("userId");

		return claim is not null && Guid.TryParse(claim.Value, out userId);
	}

	/// <summary>
	/// Gets the user's email from the ClaimsPrincipal.
	/// </summary>
	public static string? GetEmail(this ClaimsPrincipal principal)
	{
		return principal.FindFirst(ClaimTypes.Email)?.Value;
	}

	/// <summary>
	/// Gets the user's roles from the ClaimsPrincipal.
	/// </summary>
	public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
	{
		return principal.FindAll(ClaimTypes.Role)
			.Select(c => c.Value)
			.Where(v => !string.IsNullOrWhiteSpace(v));
	}
}
