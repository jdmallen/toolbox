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
	public interface IJwtTokenFactory
	{
		ClaimsIdentity GenerateClaimsIdentity(string email, string id);

		ClaimsIdentity GenerateClaimsIdentity(string email, Guid id);

		string GenerateToken(ClaimsIdentity identity);
	}

	public class JwtTokenFactory : IJwtTokenFactory
	{
		private readonly JwtOptions _jwtOptions;

		public JwtTokenFactory(IOptions<JwtOptions> jwtOptions)
		{
			_jwtOptions = jwtOptions.Value;
		}

		public ClaimsIdentity GenerateClaimsIdentity(string email, string id)
			=> new ClaimsIdentity(new GenericIdentity(email, "token"),
								new[]
								{
									new Claim(JwtClaimTypes.UserId, id),
									new Claim(ClaimTypes.Name, email),
									new Claim(ClaimTypes.Email, email),
									new Claim(JwtClaimTypes.UserRole, JwtClaims.ApiUser),
								});

		public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id) 
			=> GenerateClaimsIdentity(email, id.ToString("D"));

		public string GenerateToken(ClaimsIdentity identity)
		{
			var email = identity.Claims.Single(c => c.Type == ClaimTypes.Email).Value;

			var claims = new List<Claim>
			{
				new Claim(JwtClaimTypes.Subject, email),
				new Claim(JwtClaimTypes.JwtId, _jwtOptions.NewJti),
				new Claim(JwtClaimTypes.IssuedAt,
						"" + _jwtOptions.IssuedAt.ToUnixTimestamp(),
						ClaimValueTypes.Integer64),
				identity.FindFirst(JwtClaimTypes.UserRole),
				identity.FindFirst(JwtClaimTypes.UserId)
			};

			var token = new JwtSecurityToken(issuer: _jwtOptions.Issuer,
											audience: _jwtOptions.Audience,
											claims: claims,
											notBefore: _jwtOptions.NotBefore,
											expires: _jwtOptions.Expiration,
											signingCredentials: _jwtOptions.SigningCredentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
