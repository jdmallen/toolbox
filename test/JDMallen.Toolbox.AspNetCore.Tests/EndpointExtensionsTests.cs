using System.Net;
using JDMallen.Toolbox.AspNetCore.MinimalApi;
using JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JDMallen.Toolbox.AspNetCore.Tests;

public class EndpointExtensionsTests
{
	/// <summary>
	/// Sample endpoint used to prove that <c>MapEndpoint</c> dispatches to the
	/// type's static <see cref="IEndpoint.Map"/> implementation.
	/// </summary>
	private sealed class PingEndpoint : IEndpoint
	{
		public static void Map(IEndpointRouteBuilder app) =>
			app.MapGet("/ping", () => "pong");
	}

	[Fact]
	public async Task MapEndpoint_InvokesEndpointMap()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: app => app.MapEndpoint<PingEndpoint>());

		var response = await host.Client.GetAsync("/ping");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("pong", await response.Content.ReadAsStringAsync());
	}

	[Fact]
	public async Task MapPublicGroup_AllowsAnonymousAccess()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: app =>
			{
				var group = app.MapPublicGroup("/public");
				group.MapGet("/data", () => Results.Ok("public"));
			});

		// No auth header is sent, yet the anonymous group is reachable.
		var response = await host.Client.GetAsync("/public/data");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task MapAuthorizedGroup_RejectsAnonymousRequests()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: app =>
			{
				var group = app.MapAuthorizedGroup("/secure");
				group.MapGet("/data", () => Results.Ok("secret"));
			});

		var response = await host.Client.GetAsync("/secure/data");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task MapAuthorizedGroup_AllowsAuthenticatedRequests()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: app =>
			{
				var group = app.MapAuthorizedGroup("/secure");
				group.MapGet("/data", () => Results.Ok("secret"));
			});

		var request = new HttpRequestMessage(HttpMethod.Get, "/secure/data");
		request.Headers.Add(TestAuthHandler.UserHeader, Guid.NewGuid().ToString());

		var response = await host.Client.SendAsync(request);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}
}
