using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Constants;

namespace JDMallen.Toolbox.AspNetCore.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal objects.
/// </summary>
/// <remarks>
/// User ID lookups prefer the library's <see cref="JwtClaimTypes.UserId"/>
/// claim and fall back to the framework's
/// <see cref="ClaimTypes.NameIdentifier"/> so tokens minted by either
/// convention resolve correctly.
/// </remarks>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Gets the user ID as a Guid from the ClaimsPrincipal.
	/// </summary>
	/// <param name="principal">The principal to read the claim from.</param>
	/// <returns>The user ID parsed as a <see cref="Guid"/>.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the user ID claim is missing or is not a valid
	/// <see cref="Guid"/>.
	/// </exception>
	public static Guid GetUserIdAsGuid(this ClaimsPrincipal principal)
	{
		var claim = principal.FindFirst(JwtClaimTypes.UserId)
		            ?? principal.FindFirst(ClaimTypes.NameIdentifier);

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
	/// <param name="principal">The principal to read the claim from.</param>
	/// <returns>The user ID claim value.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the user ID claim is missing.
	/// </exception>
	public static string GetUserId(this ClaimsPrincipal principal)
	{
		return principal.FindFirst(JwtClaimTypes.UserId)?.Value
		       ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
		       ?? throw new InvalidOperationException("User ID claim not found.");
	}

	/// <summary>
	/// Tries to get the user ID as a Guid from the ClaimsPrincipal.
	/// </summary>
	/// <param name="principal">The principal to read the claim from.</param>
	/// <param name="userId">
	/// When this method returns <see langword="true"/>, contains the parsed
	/// user ID; otherwise <see cref="Guid.Empty"/>.
	/// </param>
	/// <returns>
	/// <see langword="true"/> if a valid user ID claim was found; otherwise
	/// <see langword="false"/>.
	/// </returns>
	public static bool TryGetUserIdAsGuid(
		this ClaimsPrincipal principal,
		out Guid userId)
	{
		userId = Guid.Empty;
		var claim = principal.FindFirst(JwtClaimTypes.UserId)
		            ?? principal.FindFirst(ClaimTypes.NameIdentifier);

		return claim is not null && Guid.TryParse(claim.Value, out userId);
	}

	/// <summary>
	/// Gets the user's email from the ClaimsPrincipal.
	/// </summary>
	/// <param name="principal">The principal to read the claim from.</param>
	/// <returns>The email claim value, or <see langword="null"/> if absent.</returns>
	public static string? GetEmail(this ClaimsPrincipal principal)
	{
		return principal.FindFirst(ClaimTypes.Email)?.Value
		       ?? principal.FindFirst(OtherJwtClaimTypes.Email)?.Value;
	}

	/// <summary>
	/// Gets the user's roles from the ClaimsPrincipal.
	/// </summary>
	/// <param name="principal">The principal to read the claims from.</param>
	/// <returns>The non-empty role claim values.</returns>
	public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
	{
		return principal.FindAll(JwtClaimTypes.UserRole)
			.Select(c => c.Value)
			.Where(v => !string.IsNullOrWhiteSpace(v));
	}
}
