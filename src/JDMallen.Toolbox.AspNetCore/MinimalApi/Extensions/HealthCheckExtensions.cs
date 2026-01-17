using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for health checks in minimal APIs.
/// </summary>
public static class HealthCheckExtensions
{
	/// <summary>
	/// Maps health check endpoints for liveness and readiness probes.
	/// </summary>
	public static IEndpointRouteBuilder MapHealthChecks(
		this IEndpointRouteBuilder app,
		string livenessPath = "/health/live",
		string readinessPath = "/health/ready")
	{
		throw new System.NotImplementedException();
	}
}
