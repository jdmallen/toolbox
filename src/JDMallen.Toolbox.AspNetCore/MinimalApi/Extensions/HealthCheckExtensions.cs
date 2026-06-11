using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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
	public static IEndpointRouteBuilder MapHealthCheckEndpoints(
		this IEndpointRouteBuilder app,
		string livenessPath = "/health/live",
		string readinessPath = "/health/ready")
	{
		app.MapHealthChecks(
			livenessPath,
			new HealthCheckOptions
			{
				Predicate = check => check.Tags.Contains("liveness"),
			});

		app.MapHealthChecks(
			readinessPath,
			new HealthCheckOptions
			{
				Predicate = check => check.Tags.Contains("readiness"),
			});

		return app;
	}
}
