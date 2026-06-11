using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
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
		return services.AddRateLimiter(options =>
		{
			options.AddFixedWindowLimiter(
				"auth",
				opt =>
				{
					opt.PermitLimit = permitLimit;
					opt.Window = TimeSpan.FromSeconds(windowSeconds);
					opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
					opt.QueueLimit = 2;
				});
		});
	}

	/// <summary>
	/// Applies the authentication rate limiter to an endpoint.
	/// </summary>
	public static RouteHandlerBuilder WithAuthRateLimit(this RouteHandlerBuilder builder)
		=> builder.RequireRateLimiting("auth");
}
