using System;
using System.Security.Claims;

namespace JDMallen.Toolbox.AspNetCore.Factories
{
	public interface IJwtTokenFactory
	{
		ClaimsIdentity GenerateClaimsIdentity(string email, string id);

		ClaimsIdentity GenerateClaimsIdentity(string email, Guid id);

		string GenerateToken(ClaimsIdentity identity);
	}
}
