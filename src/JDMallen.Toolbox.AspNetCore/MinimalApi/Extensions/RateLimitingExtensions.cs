using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for rate limiting in minimal APIs.
/// </summary>
public static class RateLimitingExtensions
{
	/// <summary>
	/// Adds a fixed window rate limiter for authentication endpoints.
	/// </summary>
	public static IServiceCollection AddAuthenticationRateLimiting(
		this IServiceCollection services,
		int permitLimit = 5,
		int windowSeconds = 60)
	{
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// Applies the authentication rate limiter to an endpoint.
	/// </summary>
	public static RouteHandlerBuilder WithAuthRateLimit(this RouteHandlerBuilder builder)
	{
		throw new System.NotImplementedException();
	}
}
