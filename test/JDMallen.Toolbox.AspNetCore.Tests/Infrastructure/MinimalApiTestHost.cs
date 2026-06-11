using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;

/// <summary>
/// Spins up an in-process minimal API "sample project" backed by
/// <see cref="TestServer" />. This is the sample Minimal API project the
/// modernization plan calls for in its Phase 1 testing step: each test maps the
/// endpoint helpers under test and drives them with a real
/// <see cref="HttpClient" />.
/// </summary>
internal sealed class MinimalApiTestHost : IAsyncDisposable
{
	private readonly WebApplication _app;

	private MinimalApiTestHost(WebApplication app)
	{
		_app = app;
	}

	/// <summary>An <see cref="HttpClient" /> bound to the test server.</summary>
	public HttpClient Client { get; private init; } = null!;

	public async ValueTask DisposeAsync()
	{
		Client.Dispose();
		await _app.DisposeAsync();
	}

	/// <summary>
	/// Builds and starts a test host, registering the test authentication
	/// scheme plus authorization, then invokes the caller's configuration.
	/// </summary>
	/// <param name="configureServices">
	/// Optional hook to register additional services (e.g. validators).
	/// </param>
	/// <param name="configureEndpoints">
	/// Maps the endpoints to exercise onto the application.
	/// </param>
	public static async Task<MinimalApiTestHost> StartAsync(
		Action<IServiceCollection>? configureServices,
		Action<IEndpointRouteBuilder> configureEndpoints)
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
		builder.WebHost.UseTestServer();

		builder.Services
			.AddAuthentication(TestAuthHandler.SchemeName)
			.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
				TestAuthHandler.SchemeName,
				_ => { });
		builder.Services.AddAuthorization();

		configureServices?.Invoke(builder.Services);

		WebApplication app = builder.Build();
		app.UseAuthentication();
		app.UseAuthorization();

		configureEndpoints(app);

		await app.StartAsync();

		return new MinimalApiTestHost(app)
		{
			Client = app.GetTestClient(),
		};
	}
}
