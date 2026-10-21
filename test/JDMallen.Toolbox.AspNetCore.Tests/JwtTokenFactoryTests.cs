using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Factories;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Tests for the identities <see cref="JwtTokenFactory" /> builds and the
/// claims it stamps on each token.
/// </summary>
public class JwtTokenFactoryTests
{
	private static JwtTokenFactory CreateFactory() =>
		new(
			Microsoft.Extensions.Options.Options.Create(
				new JwtOptions
				{
					Issuer = "https://issuer.example.com",
					Audience = "https://audience.example.com",
					Subject = "tests",
				}),
			new JwtSigningCredentials(
				new SigningCredentials(
					new SymmetricSecurityKey(new byte[32]),
					SecurityAlgorithms.HmacSha256)));

	private static JwtSecurityToken Read(string token) =>
		new JwtSecurityTokenHandler().ReadJwtToken(token);

	[Fact]
	public void GenerateToken_StampsIssuedAtInWholeUnixSeconds()
	{
		JwtTokenFactory factory = CreateFactory();
		var identity = new ClaimsIdentity(
			[new Claim(ClaimTypes.Name, "user@example.com")]);

		long before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		string token = factory.GenerateToken(identity);
		long after = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		// 4.0 truncates to the second, as DateTimeOffset.ToUnixTimeSeconds does;
		// 3.x rounded up, which could stamp an iat up to a second in the future.
		Claim issuedAt = Read(token)
			.Claims
			.Single(claim => claim.Type == JwtClaimTypes.IssuedAt);

		Assert.Equal(ClaimValueTypes.Integer64, issuedAt.ValueType);
		Assert.InRange(
			long.Parse(issuedAt.Value, CultureInfo.InvariantCulture),
			before,
			after);
	}

	[Fact]
	public void GenerateClaimsIdentity_HasOneNameClaim()
	{
		// 3.x wrapped a GenericIdentity, which already carries a Name claim, and
		// added a second one, so GenerateToken threw on its own identity (B8).
		ClaimsIdentity identity = CreateFactory()
			.GenerateClaimsIdentity("user@example.com", Guid.NewGuid());

		Assert.Single(identity.FindAll(ClaimTypes.Name));
		Assert.Equal("user@example.com", identity.Name);
		Assert.True(identity.IsAuthenticated);
	}

	[Fact]
	public void GenerateToken_CarriesTheUserIdAndEmailOfItsOwnIdentity()
	{
		// 3.x wrote the email (the Name claim) into "id" and dropped the real
		// user ID (B2).
		JwtTokenFactory factory = CreateFactory();
		var userId = Guid.NewGuid();

		JwtSecurityToken token = Read(
			factory.GenerateToken(
				factory.GenerateClaimsIdentity("user@example.com", userId)));

		Assert.Equal(
			userId.ToString("D"),
			token.Claims.Single(claim => claim.Type == JwtClaimTypes.UserId).Value);
		Assert.Equal(
			"user@example.com",
			token.Claims.Single(claim => claim.Type == OtherJwtClaimTypes.Email).Value);
	}

	[Fact]
	public void GenerateToken_FallsBackToTheNameIdentifierForTheUserId()
	{
		var identity = new ClaimsIdentity(
			[new Claim(ClaimTypes.NameIdentifier, "42")]);

		JwtSecurityToken token = Read(CreateFactory().GenerateToken(identity));

		Assert.Equal(
			"42",
			token.Claims.Single(claim => claim.Type == JwtClaimTypes.UserId).Value);
	}

	[Fact]
	public void GenerateToken_OmitsTheUserIdWhenTheIdentityHasNone()
	{
		var identity = new ClaimsIdentity(
			[new Claim(ClaimTypes.Name, "user@example.com")]);

		JwtSecurityToken token = Read(CreateFactory().GenerateToken(identity));

		Assert.DoesNotContain(
			token.Claims,
			claim => claim.Type == JwtClaimTypes.UserId);
	}
}
