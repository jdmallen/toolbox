using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using JDMallen.Toolbox.Constants;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Options;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.Factories
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
			var email = identity.Claims.Single(c => c.Type == ClaimTypes.Email).Value;

			var claims = new List<Claim>
			{
				new Claim(JwtClaimTypes.Subject, email),
				new Claim(JwtClaimTypes.JwtId, JwtOptions.NewJti),
				new Claim(
					JwtClaimTypes.IssuedAt,
					"" + JwtOptions.IssuedAt.ToUnixTimestamp(),
					ClaimValueTypes.Integer64),
				identity.FindFirst(JwtClaimTypes.UserId)
			};
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
