using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using JDMallen.Toolbox.AspNetCore.Constants;
using JDMallen.Toolbox.AspNetCore.Options;
using JDMallen.Toolbox.Extensions;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.AspNetCore.Factories
{
	public class JwtTokenFactory : IJwtTokenFactory
	{
		protected readonly JwtOptions JwtOptions;

		public JwtTokenFactory(IOptions<JwtOptions> jwtOptions)
		{
			JwtOptions = jwtOptions.Value;
		}

		public ClaimsIdentity GenerateClaimsIdentity(string email, string id)
			=> new ClaimsIdentity(
				new GenericIdentity(email, "token"),
				new[]
				{
					new Claim(JwtClaimTypes.UserId, id),
					new Claim(ClaimTypes.Name, email),
					new Claim(ClaimTypes.Email, email),
				});

		public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id)
			=> GenerateClaimsIdentity(email, id.ToString("D"));

		public string GenerateToken(ClaimsIdentity identity)
		{
			var userName =
				identity
					.Claims
					.SingleOrDefault(c => c.Type == ClaimTypes.Name)
					?.Value;

			var claims = new List<Claim>
			{
				new Claim(JwtClaimTypes.JwtId, JwtOptions.NewJti),
				new Claim(
					JwtClaimTypes.IssuedAt,
					"" + JwtOptions.IssuedAt.ToUnixTimestamp(),
					ClaimValueTypes.Integer64)
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
				issuer: JwtOptions.Issuer,
				audience: JwtOptions.Audience,
				claims: claims.Where(x => x != null).ToList(),
				notBefore: JwtOptions.NotBefore,
				expires: JwtOptions.Expiration,
				signingCredentials: JwtOptions.SigningCredentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
