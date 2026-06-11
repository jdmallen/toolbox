using System.Net;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that the authentication rate limiter registered by
/// <see cref="RateLimitingExtensions.AddAuthenticationRateLimiting"/> and applied
/// via <see cref="RateLimitingExtensions.WithAuthRateLimit"/> rejects traffic
/// once the permit window is exhausted.
/// </summary>
public class RateLimitingExtensionsTests
{
	// The rate limiter middleware needs an explicit pipeline stage, so this test
	// builds its own host rather than reusing the shared MinimalApiTestHost.
	private static async Task<(WebApplication App, HttpClient Client)> StartLimitedHostAsync()
	{
		var builder = WebApplication.CreateSlimBuilder();
		builder.WebHost.UseTestServer();

		// A single permit with a one-second window keeps the test fast while still
		// forcing rejections once the concurrent burst exceeds permits + queue.
		builder.Services.AddAuthenticationRateLimiting(
			permitLimit: 1,
			windowSeconds: 1);

		var app = builder.Build();
		app.UseRateLimiter();
		app.MapGet("/auth", () => Results.Ok("ok")).WithAuthRateLimit();

		await app.StartAsync();
		return (app, app.GetTestClient());
	}

	[Fact]
	public async Task WithAuthRateLimit_RejectsRequestsBeyondThePermitAndQueue()
	{
		var (app, client) = await StartLimitedHostAsync();
		try
		{
			// One permit plus a queue of two leaves any further concurrent
			// requests to be rejected outright.
			var responses = await Task.WhenAll(
				Enumerable
					.Range(0, 6)
					.Select(_ => client.GetAsync("/auth")));

			var statuses = responses.Select(r => r.StatusCode).ToArray();

			// The framework default rejection status for rate limiting is 503.
			Assert.Contains(HttpStatusCode.ServiceUnavailable, statuses);
			Assert.Contains(HttpStatusCode.OK, statuses);

			foreach (var response in responses)
			{
				response.Dispose();
			}
		}
		finally
		{
			client.Dispose();
			await app.DisposeAsync();
		}
	}

	[Fact]
	public async Task SingleRequest_IsAllowed()
	{
		var (app, client) = await StartLimitedHostAsync();
		try
		{
			var response = await client.GetAsync("/auth");

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			response.Dispose();
		}
		finally
		{
			client.Dispose();
			await app.DisposeAsync();
		}
	}
}
