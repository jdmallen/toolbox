using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.AspNetCore.Factories;

/// <summary>
/// Factory for creating JWT tokens and claims identities for authentication.
/// </summary>
public class JwtTokenFactory : IJwtTokenFactory
{
	/// <summary>
	/// Initializes a new instance of the <see cref="JwtTokenFactory" /> class.
	/// </summary>
	/// <param name="jwtOptions">The JWT configuration options.</param>
	/// <param name="signingCredentials">The credentials to sign tokens with.</param>
	public JwtTokenFactory(
		IOptions<JwtOptions> jwtOptions,
		JwtSigningCredentials signingCredentials)
	{
		ArgumentNullException.ThrowIfNull(jwtOptions);
		ArgumentNullException.ThrowIfNull(signingCredentials);
		JwtOptions = jwtOptions.Value;
		SigningCredentials = signingCredentials;
	}

	/// <summary>
	/// Gets the JWT options used for token generation and validation.
	/// </summary>
	protected JwtOptions JwtOptions { get; }

	/// <summary>
	/// Gets the credentials tokens are signed with.
	/// </summary>
	protected JwtSigningCredentials SigningCredentials { get; }

	/// <inheritdoc />
	public ClaimsIdentity GenerateClaimsIdentity(string email, string id) => new(
		[
			new Claim(JwtClaimTypes.UserId, id),
			new Claim(ClaimTypes.Name, email),
			new Claim(ClaimTypes.Email, email),
		],
		"token");

	/// <inheritdoc />
	public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id)
		=> GenerateClaimsIdentity(email, id.ToString("D"));

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity identity)
	{
		ArgumentNullException.ThrowIfNull(identity);

		// The same lookup order ClaimsPrincipalExtensions.GetUserId reads back.
		string? userId =
			(identity.FindFirst(JwtClaimTypes.UserId)
				?? identity.FindFirst(ClaimTypes.NameIdentifier))
			?.Value;

		var claims = new List<Claim?>
		{
			new(JwtClaimTypes.JwtId, JwtOptions.NewJti),
			new(
				JwtClaimTypes.IssuedAt,
				new DateTimeOffset(JwtOptions.IssuedAt)
					.ToUnixTimeSeconds()
					.ToString(CultureInfo.InvariantCulture),
				ClaimValueTypes.Integer64),
		};

		if (!string.IsNullOrWhiteSpace(userId))
		{
			claims.Add(new Claim(JwtClaimTypes.UserId, userId));
		}

		claims.Add(
			identity.FindAll(ClaimTypes.PrimarySid)
				.Select(claim => new Claim(OtherJwtClaimTypes.PrimarySid, claim.Value))
				.FirstOrDefault());

		claims.Add(
			identity.FindAll(ClaimTypes.Email)
				.Select(claim => new Claim(OtherJwtClaimTypes.Email, claim.Value))
				.FirstOrDefault());

		claims.AddRange(identity.FindAll(JwtClaimTypes.UserRole));

		var token = new JwtSecurityToken(
			JwtOptions.Issuer,
			JwtOptions.Audience,
			[.. claims.Where(x => x != null)],
			JwtOptions.NotBefore,
			JwtOptions.Expiration,
			SigningCredentials.Credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
