using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for OpenAPI/Swagger configuration in minimal APIs.
/// </summary>
public static class OpenApiExtensions
{
	/// <summary>
	/// Adds OpenAPI with JWT Bearer authentication configuration.
	/// </summary>
	public static IServiceCollection AddOpenApiWithJwt(
		this IServiceCollection services,
		string title,
		string version = "v1")
	{
		return services.AddOpenApi(options =>
		{
			options.AddDocumentTransformer((document, _, _) =>
			{
				document.Info = new OpenApiInfo
				{
					Title = title,
					Version = version,
				};

				document.Components ??= new OpenApiComponents();

				// SecuritySchemes is null until first assigned, so the dictionary
				// must be created before indexing into it; a null-conditional
				// indexer assignment would silently drop the scheme.
				document.Components.SecuritySchemes ??=
					new Dictionary<string, IOpenApiSecurityScheme>();
				document.Components.SecuritySchemes["Bearer"] =
					new OpenApiSecurityScheme
					{
						Type = SecuritySchemeType.Http,
						Scheme = "bearer",
						BearerFormat = "JWT",
						Description = "Enter your JWT token",
					};

				return Task.CompletedTask;
			});
		});
	}

	/// <summary>
	/// Adds common responses to endpoint metadata (401, 403, 500).
	/// </summary>
	public static RouteHandlerBuilder WithCommonResponses(
		this RouteHandlerBuilder builder,
		bool includeAuth = false)
	{
		builder.Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

		if (includeAuth)
		{
			builder
				.Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
				.Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
		}

		return builder;
	}
}
