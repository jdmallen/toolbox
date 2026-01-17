using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi;

/// <summary>
/// Defines a minimal API endpoint with a static Map method for registration.
/// </summary>
public interface IEndpoint
{
	static abstract void Map(IEndpointRouteBuilder app);
}
