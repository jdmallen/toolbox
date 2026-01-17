using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi;

/// <summary>
/// Defines a minimal API endpoint with a static Map method for registration.
/// </summary>
public interface IEndpoint
{
	/// <summary>
	/// Maps the endpoint routes to the application's route builder.
	/// </summary>
	/// <param name="app">The endpoint route builder used to register routes.</param>
	static abstract void Map(IEndpointRouteBuilder app);
}
