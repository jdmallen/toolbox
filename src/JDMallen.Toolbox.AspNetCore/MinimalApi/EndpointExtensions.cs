using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi;

/// <summary>
/// Extension methods for registering minimal API endpoints.
/// </summary>
public static class EndpointExtensions
{
	extension(IEndpointRouteBuilder app)
	{
		/// <summary>
		/// Creates a route group that requires authorization.
		/// </summary>
		public RouteGroupBuilder MapAuthorizedGroup(string? prefix = null)
		{
			RouteGroupBuilder group = app.MapGroup(prefix ?? string.Empty)
				.RequireAuthorization();

			return group;
		}

		/// <summary>
		/// Maps an endpoint implementing IEndpoint to the route builder.
		/// </summary>
		public IEndpointRouteBuilder MapEndpoint<TEndpoint>()
			where TEndpoint : IEndpoint
		{
			TEndpoint.Map(app);

			return app;
		}

		/// <summary>
		/// Creates a route group that allows anonymous access.
		/// </summary>
		public RouteGroupBuilder MapPublicGroup(string? prefix = null)
			=> app.MapGroup(prefix ?? string.Empty)
				.AllowAnonymous();
	}
}
