using System.Net;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="HealthCheckExtensions.MapHealthCheckEndpoints" />
/// maps separate liveness and readiness probes, each filtered to its own tag.
/// </summary>
public class HealthCheckExtensionsTests
{
	// A host whose liveness check is healthy and whose readiness check is not,
	// so the two probes must report different statuses if tag filtering works.
	private static Task<MinimalApiTestHost> StartProbeHostAsync() =>
		MinimalApiTestHost.StartAsync(
			services => services
				.AddHealthChecks()
				.AddCheck(
					"live",
					() => HealthCheckResult.Healthy(),
					["liveness"])
				.AddCheck(
					"ready",
					() => HealthCheckResult.Unhealthy("Not ready yet."),
					["readiness"]),
			app => app.MapHealthCheckEndpoints());

	[Fact]
	public async Task CustomPaths_AreHonored()
	{
		await using MinimalApiTestHost host = await MinimalApiTestHost.StartAsync(
			services => services
				.AddHealthChecks()
				.AddCheck("live", () => HealthCheckResult.Healthy(), ["liveness"]),
			app => app.MapHealthCheckEndpoints(
				"/alive",
				"/ready"));

		HttpResponseMessage response = await host.Client.GetAsync("/alive");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task LivenessProbe_ReportsHealthy()
	{
		await using MinimalApiTestHost host = await StartProbeHostAsync();

		HttpResponseMessage response = await host.Client.GetAsync("/health/live");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task ReadinessProbe_ReportsUnhealthyIndependentlyOfLiveness()
	{
		await using MinimalApiTestHost host = await StartProbeHostAsync();

		HttpResponseMessage response = await host.Client.GetAsync("/health/ready");

		Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
	}
}
