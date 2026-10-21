using JDMallen.Toolbox.AspNetCore.Factories;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for JWT authentication in minimal APIs.
/// </summary>
public static class JwtAuthenticationExtensions
{
	/// <summary>
	/// Configures JWT Bearer authentication for Minimal APIs and MVC, and
	/// registers an <see cref="IJwtTokenFactory" /> that issues tokens this
	/// configuration accepts. The issuer, audience, and other string settings
	/// are bound from the named configuration section into
	/// <see cref="JwtOptions" />. The signing credentials are passed in, because
	/// a <see cref="SecurityKey" /> cannot be bound from configuration, and are
	/// registered as <see cref="JwtSigningCredentials" /> for both validation
	/// and signing.
	/// </summary>
	/// <remarks>
	/// <see cref="IJwtTokenFactory" /> is registered with <c>TryAdd</c>, so a
	/// factory registered earlier wins.
	/// </remarks>
	/// <param name="services">The service collection to add to.</param>
	/// <param name="configuration">The configuration holding the JWT section.</param>
	/// <param name="signingCredentials">
	/// The credentials that sign issued tokens and validate incoming ones.
	/// </param>
	/// <param name="jwtSectionName">The configuration section to bind.</param>
	/// <returns>The authentication builder, for chaining more schemes.</returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="signingCredentials" /> is null.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The configuration section is missing or empty.
	/// </exception>
	public static AuthenticationBuilder AddJwtAuthentication(
		this IServiceCollection services,
		IConfiguration configuration,
		SigningCredentials signingCredentials,
		string jwtSectionName = "Jwt")
	{
		var jwtSigningCredentials = new JwtSigningCredentials(signingCredentials);

		IConfigurationSection jwtSection = configuration.GetSection(jwtSectionName);
		JwtOptions jwtOptions = jwtSection.Get<JwtOptions>()
			?? throw new InvalidOperationException(
				$"JWT configuration section '{jwtSectionName}' is missing or empty.");

		services.Configure<JwtOptions>(jwtSection);
		services.AddSingleton(jwtSigningCredentials);
		services.TryAddSingleton<IJwtTokenFactory, JwtTokenFactory>();

		return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = jwtSigningCredentials.Credentials.Key,
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,
					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.FromMinutes(5),
				};
			});
	}
}
