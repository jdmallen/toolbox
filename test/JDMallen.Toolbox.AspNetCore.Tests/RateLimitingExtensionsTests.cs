using System.Net;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that the authentication rate limiter registered by
/// <see cref="RateLimitingExtensions.AddAuthenticationRateLimiting" /> and applied
/// via <see cref="RateLimitingExtensions.WithAuthRateLimit" /> rejects traffic
/// once the permit window is exhausted.
/// </summary>
public class RateLimitingExtensionsTests
{
	// The rate limiter middleware needs an explicit pipeline stage, so this test
	// builds its own host rather than reusing the shared MinimalApiTestHost.
	private static async Task<(WebApplication App, HttpClient Client)> StartLimitedHostAsync()
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
		builder.WebHost.UseTestServer();

		// A single permit with a one-second window keeps the test fast while still
		// forcing rejections once the concurrent burst exceeds permits + queue.
		builder.Services.AddAuthenticationRateLimiting(
			1,
			1);

		WebApplication app = builder.Build();
		app.UseRateLimiter();
		app.MapGet("/auth", () => Results.Ok("ok")).WithAuthRateLimit();

		await app.StartAsync();

		return (app, app.GetTestClient());
	}

	// Issuing the concurrent burst from a dedicated scope keeps the request
	// closure away from the caller's client disposal, so the captured client is
	// never seen as disposed in the same scope.
	private static async Task<HttpStatusCode[]> SendConcurrentBurstAsync(
		HttpClient client,
		int requestCount)
	{
		HttpResponseMessage[] responses = await Task.WhenAll(
			Enumerable
				.Range(0, requestCount)
				.Select(_ => client.GetAsync("/auth")));

		try
		{
			return responses.Select(response => response.StatusCode).ToArray();
		}
		finally
		{
			foreach (HttpResponseMessage response in responses)
			{
				response.Dispose();
			}
		}
	}

	[Fact]
	public async Task SingleRequest_IsAllowed()
	{
		(WebApplication app, HttpClient client) = await StartLimitedHostAsync();
		try
		{
			HttpResponseMessage response = await client.GetAsync("/auth");

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			response.Dispose();
		}
		finally
		{
			client.Dispose();
			await app.DisposeAsync();
		}
	}

	[Fact]
	public async Task WithAuthRateLimit_RejectsRequestsBeyondThePermitAndQueue()
	{
		(WebApplication app, HttpClient client) = await StartLimitedHostAsync();
		try
		{
			// One permit plus a queue of two leaves any further concurrent
			// requests to be rejected outright.
			HttpStatusCode[] statuses = await SendConcurrentBurstAsync(client, 6);

			// The framework default rejection status for rate limiting is 503.
			Assert.Contains(HttpStatusCode.ServiceUnavailable, statuses);
			Assert.Contains(HttpStatusCode.OK, statuses);
		}
		finally
		{
			client.Dispose();
			await app.DisposeAsync();
		}
	}
}
