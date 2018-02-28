using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using JDMallen.Toolbox.Constants;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Options;
using Microsoft.Extensions.Options;

namespace JDMallen.Toolbox.Factories
{
	public interface IJwtTokenFactory
	{
		string GenerateToken(ClaimsIdentity identity);
	}

	public class JwtTokenFactory : IJwtTokenFactory
	{
		private readonly JwtOptions _jwtOptions;

		public JwtTokenFactory(IOptions<JwtOptions> jwtOptions)
		{
			_jwtOptions = jwtOptions.Value;
		}

		public string GenerateToken(ClaimsIdentity identity)
		{
			var username = identity.Claims.Single(c => c.Type == ClaimTypes.Name).Value;

			var claims = new List<Claim>
			{
				new Claim(JwtClaimTypes.Subject, username),
				new Claim(JwtClaimTypes.JwtId, _jwtOptions.NewJti),
				new Claim(JwtClaimTypes.IssuedAt,
						"" + _jwtOptions.IssuedAt.ToUnixTimestamp(),
						ClaimValueTypes.Integer64),
				identity.FindFirst(JwtClaimTypes.UserRoleId),
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
