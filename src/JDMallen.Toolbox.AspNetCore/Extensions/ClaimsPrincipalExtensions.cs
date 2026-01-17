using System;
using System.Collections.Generic;
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
		throw new NotImplementedException();
	}

	/// <summary>
	/// Gets the user ID as a string from the ClaimsPrincipal.
	/// </summary>
	public static string GetUserId(this ClaimsPrincipal principal)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Tries to get the user ID as a Guid from the ClaimsPrincipal.
	/// </summary>
	public static bool TryGetUserIdAsGuid(this ClaimsPrincipal principal, out Guid userId)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Gets the user's email from the ClaimsPrincipal.
	/// </summary>
	public static string? GetEmail(this ClaimsPrincipal principal)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Gets the user's roles from the ClaimsPrincipal.
	/// </summary>
	public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
	{
		throw new NotImplementedException();
	}
}
