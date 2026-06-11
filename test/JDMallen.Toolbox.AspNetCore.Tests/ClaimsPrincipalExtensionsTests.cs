using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Extensions;

namespace JDMallen.Toolbox.AspNetCore.Tests;

public class ClaimsPrincipalExtensionsTests
{
	private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
		new(new ClaimsIdentity(claims, "test"));

	[Fact]
	public void GetUserId_PrefersJwtUserIdClaim()
	{
		var principal = PrincipalWith(
			new Claim(JwtClaimTypes.UserId, "primary"),
			new Claim(ClaimTypes.NameIdentifier, "fallback"));

		Assert.Equal("primary", principal.GetUserId());
	}

	[Fact]
	public void GetUserId_FallsBackToNameIdentifier()
	{
		var principal = PrincipalWith(
			new Claim(ClaimTypes.NameIdentifier, "fallback"));

		Assert.Equal("fallback", principal.GetUserId());
	}

	[Fact]
	public void GetUserId_ThrowsWhenMissing()
	{
		var principal = PrincipalWith();

		Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
	}

	[Fact]
	public void GetUserIdAsGuid_ParsesGuidClaim()
	{
		var expected = Guid.NewGuid();
		var principal = PrincipalWith(
			new Claim(JwtClaimTypes.UserId, expected.ToString()));

		Assert.Equal(expected, principal.GetUserIdAsGuid());
	}

	[Fact]
	public void GetUserIdAsGuid_ThrowsWhenNotAGuid()
	{
		var principal = PrincipalWith(
			new Claim(JwtClaimTypes.UserId, "not-a-guid"));

		Assert.Throws<InvalidOperationException>(
			() => principal.GetUserIdAsGuid());
	}

	[Fact]
	public void TryGetUserIdAsGuid_ReturnsTrueForValidGuid()
	{
		var expected = Guid.NewGuid();
		var principal = PrincipalWith(
			new Claim(ClaimTypes.NameIdentifier, expected.ToString()));

		Assert.True(principal.TryGetUserIdAsGuid(out var actual));
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void TryGetUserIdAsGuid_ReturnsFalseAndEmptyForMissingClaim()
	{
		var principal = PrincipalWith();

		Assert.False(principal.TryGetUserIdAsGuid(out var actual));
		Assert.Equal(Guid.Empty, actual);
	}

	[Fact]
	public void GetEmail_PrefersStandardEmailClaim()
	{
		var principal = PrincipalWith(
			new Claim(ClaimTypes.Email, "standard@example.com"),
			new Claim(OtherJwtClaimTypes.Email, "jwt@example.com"));

		Assert.Equal("standard@example.com", principal.GetEmail());
	}

	[Fact]
	public void GetEmail_FallsBackToJwtEmailClaim()
	{
		var principal = PrincipalWith(
			new Claim(OtherJwtClaimTypes.Email, "jwt@example.com"));

		Assert.Equal("jwt@example.com", principal.GetEmail());
	}

	[Fact]
	public void GetEmail_ReturnsNullWhenAbsent()
	{
		var principal = PrincipalWith();

		Assert.Null(principal.GetEmail());
	}

	[Fact]
	public void GetRoles_ReturnsNonEmptyRoleClaims()
	{
		var principal = PrincipalWith(
			new Claim(JwtClaimTypes.UserRole, "admin"),
			new Claim(JwtClaimTypes.UserRole, "user"),
			new Claim(JwtClaimTypes.UserRole, "   "));

		Assert.Equal(["admin", "user"], principal.GetRoles());
	}

	[Fact]
	public void GetRoles_ReturnsEmptyWhenNoRoleClaims()
	{
		var principal = PrincipalWith(
			new Claim(JwtClaimTypes.UserId, "id"));

		Assert.Empty(principal.GetRoles());
	}
}
