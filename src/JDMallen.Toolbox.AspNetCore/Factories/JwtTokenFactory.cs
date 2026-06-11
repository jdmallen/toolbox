using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.Extensions;
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
	public JwtTokenFactory(IOptions<JwtOptions> jwtOptions)
	{
		JwtOptions = jwtOptions.Value;
	}

	/// <summary>
	/// Gets the JWT options used for token generation and validation.
	/// </summary>
	protected JwtOptions JwtOptions { get; }

	/// <inheritdoc />
	public ClaimsIdentity GenerateClaimsIdentity(string email, string id) => new(
		new GenericIdentity(email, "token"),
		[
			new Claim(JwtClaimTypes.UserId, id),
			new Claim(ClaimTypes.Name, email),
			new Claim(ClaimTypes.Email, email),
		]);

	/// <inheritdoc />
	public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id)
		=> GenerateClaimsIdentity(email, id.ToString("D"));

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity identity)
	{
		string? userName =
			identity
				.Claims
				.SingleOrDefault(c => c.Type == ClaimTypes.Name)
				?.Value;

		var claims = new List<Claim?>
		{
			new(JwtClaimTypes.JwtId, JwtOptions.NewJti),
			new(
				JwtClaimTypes.IssuedAt,
				"" + JwtOptions.IssuedAt.ToUnixTimestamp(),
				ClaimValueTypes.Integer64),
		};

		if (!string.IsNullOrWhiteSpace(userName))
		{
			claims.Add(new Claim(JwtClaimTypes.UserId, userName));
		}

		//			if (!string.IsNullOrWhiteSpace(email))
		//			{
		//				claims.Add(new Claim(JwtClaimTypes.Subject, email));
		//			}

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
			claims.Where(x => x != null).ToList(),
			JwtOptions.NotBefore,
			JwtOptions.Expiration,
			JwtOptions.SigningCredentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
