using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Adds common responses to endpoint metadata (401, 403, 500).
	/// </summary>
	public static RouteHandlerBuilder WithCommonResponses(
		this RouteHandlerBuilder builder,
		bool includeAuth = false)
	{
		throw new System.NotImplementedException();
	}
}
