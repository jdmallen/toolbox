using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Factories;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Tests for the claims <see cref="JwtTokenFactory" /> stamps on each token.
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
					SigningCredentials = new SigningCredentials(
						new SymmetricSecurityKey(new byte[32]),
						SecurityAlgorithms.HmacSha256),
				}));

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
		Claim issuedAt = new JwtSecurityTokenHandler()
			.ReadJwtToken(token)
			.Claims
			.Single(claim => claim.Type == JwtClaimTypes.IssuedAt);

		Assert.Equal(ClaimValueTypes.Integer64, issuedAt.ValueType);
		Assert.InRange(
			long.Parse(issuedAt.Value, CultureInfo.InvariantCulture),
			before,
			after);
	}

	[Fact]
	public void GenerateToken_ThrowsForItsOwnClaimsIdentity()
	{
		// Pinned bug (B8): GenerateClaimsIdentity wraps a GenericIdentity, which
		// already carries a Name claim, then adds a second one. GenerateToken's
		// SingleOrDefault on Name then throws. Phase 4 rewrites both methods
		// (with B2); flip this test then.
		JwtTokenFactory factory = CreateFactory();
		ClaimsIdentity identity =
			factory.GenerateClaimsIdentity("user@example.com", Guid.NewGuid());

		Assert.Equal(2, identity.FindAll(ClaimTypes.Name).Count());
		Assert.Throws<InvalidOperationException>(
			() => factory.GenerateToken(identity));
	}
}
