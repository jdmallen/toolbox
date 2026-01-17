using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for JWT authentication in minimal APIs.
/// </summary>
public static class JwtAuthenticationExtensions
{
	/// <summary>
	/// Configures JWT Bearer authentication for Minimal APIs and MVC.
	/// </summary>
	public static AuthenticationBuilder AddJwtAuthentication(
		this IServiceCollection services,
		IConfiguration configuration,
		string jwtSectionName = "Jwt")
	{
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Adds a RouteHandlerBuilder extension to require JWT authentication with proper OpenAPI metadata.
	/// </summary>
	public static RouteHandlerBuilder RequireJwtAuth(this RouteHandlerBuilder builder)
	{
		throw new System.NotImplementedException();
	}
}
