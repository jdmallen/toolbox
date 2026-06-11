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
	/// Configures JWT Bearer authentication for Minimal APIs and MVC. The issuer,
	/// audience, and other string claims are bound from the named configuration
	/// section, while the signing credentials must be supplied via
	/// <paramref name="signingCredentials" /> because a <see cref="SecurityKey" />
	/// cannot be bound from configuration.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the configuration section is missing, or when no signing
	/// credentials are available from either the argument or the bound options.
	/// </exception>
	public static AuthenticationBuilder AddJwtAuthentication(
		this IServiceCollection services,
		IConfiguration configuration,
		string jwtSectionName = "Jwt",
		SigningCredentials? signingCredentials = null)
	{
		IConfigurationSection jwtSection = configuration.GetSection(jwtSectionName);
		JwtOptions jwtOptions = jwtSection.Get<JwtOptions>()
			?? throw new InvalidOperationException(
				$"JWT configuration section '{jwtSectionName}' is missing or empty.");

		services.Configure<JwtOptions>(jwtSection);

		// SigningCredentials cannot be bound from configuration (its SecurityKey
		// is abstract), so prefer the explicitly supplied value and fall back to
		// anything assembled on the bound options. The property is non-nullable,
		// but reflection-based binding can still leave it null at runtime, hence
		// the nullable local and the explicit guard below.
		SigningCredentials? resolvedSigningCredentials =
			signingCredentials ?? jwtOptions.SigningCredentials;

		if (resolvedSigningCredentials is null)
		{
			throw new InvalidOperationException(
				"JWT signing credentials were not supplied. Pass them via the "
				+ $"'{nameof(signingCredentials)}' parameter; they cannot be bound "
				+ "from configuration.");
		}

		return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = resolvedSigningCredentials.Key,
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,
					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromMinutes(5),
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
	public static RouteHandlerBuilder RequireJwtAuth(this RouteHandlerBuilder builder)
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
				{
					{ securitySchemeReference, [] },
				};

				operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };

				return operation;
			});
	}
}
