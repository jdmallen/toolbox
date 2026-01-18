using System.Net;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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
		var jwtOptions = configuration.GetSection(jwtSectionName).Get<JwtOptions>();
		services.Configure<JwtOptions>(configuration.GetSection(jwtSectionName));

		return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = jwtOptions?.SigningCredentials.Key,
					ValidateIssuer = true,
					ValidIssuer = jwtOptions?.Issuer,
					ValidateAudience = true,
					ValidAudience = jwtOptions?.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromMinutes(5)
				};
			});
	}

	/// <summary>
	/// Adds a RouteHandlerBuilder extension to require JWT authentication with
	/// proper OpenAPI metadata.
	/// </summary>
	[Obsolete(
		"This uses `WithOpenApi` which is deprecated. "
		+ "Consider using Swashbuckle or another OpenAPI library for better support.")]
	public static RouteHandlerBuilder RequireJwtAuth(
		this RouteHandlerBuilder builder)
	{
		return builder
			.RequireAuthorization()
			.Produces<ProblemHttpResult>(
				HttpStatusCode.Unauthorized.GetHashCode())
			.WithOpenApi(operation =>
			{
				var securitySchemeReference = new OpenApiSecuritySchemeReference(
					JwtBearerDefaults.AuthenticationScheme);

				var securityRequirement = new OpenApiSecurityRequirement
					{ { securitySchemeReference, [] } };


				operation.Security = new List<OpenApiSecurityRequirement>
					{ securityRequirement };
				return operation;
			});
	}
}
