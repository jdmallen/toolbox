using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi;

/// <summary>
/// Extension methods for registering minimal API endpoints.
/// </summary>
public static class EndpointExtensions
{
	/// <summary>
	/// Maps an endpoint implementing IEndpoint to the route builder.
	/// </summary>
	public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(
		this IEndpointRouteBuilder app)
		where TEndpoint : IEndpoint
	{
		TEndpoint.Map(app);
		return app;
	}

	/// <summary>
	/// Creates a route group that allows anonymous access.
	/// </summary>
	public static RouteGroupBuilder MapPublicGroup(
		this IEndpointRouteBuilder app,
		string? prefix = null)
	{
		return app.MapGroup(prefix ?? string.Empty)
			.AllowAnonymous();
	}

	/// <summary>
	/// Creates a route group that requires authorization.
	/// </summary>
	public static RouteGroupBuilder MapAuthorizedGroup(
		this IEndpointRouteBuilder app,
		string? prefix = null,
		Action<RouteHandlerBuilder>? configureOpenApi = null)
	{
		var group = app.MapGroup(prefix ?? string.Empty)
			.RequireAuthorization();

		return group;
	}
}
