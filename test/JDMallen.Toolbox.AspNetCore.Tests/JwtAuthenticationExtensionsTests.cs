using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using JDMallen.Toolbox.AspNetCore.Extensions;
using JDMallen.Toolbox.AspNetCore.Factories;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="JwtAuthenticationExtensions.AddJwtAuthentication" />
/// registers the JWT bearer scheme, binds <see cref="JwtOptions" /> from
/// configuration, applies the supplied signing credentials to the token
/// validation parameters, fails fast when no credentials are supplied, and
/// registers a token factory whose tokens it accepts.
/// </summary>
public class JwtAuthenticationExtensionsTests
{
	private sealed class StubTokenFactory : IJwtTokenFactory
	{
		public ClaimsIdentity GenerateClaimsIdentity(string email, string id)
			=> throw new NotSupportedException();

		public ClaimsIdentity GenerateClaimsIdentity(string email, Guid id)
			=> throw new NotSupportedException();

		public string GenerateToken(ClaimsIdentity identity)
			=> throw new NotSupportedException();
	}

	private static SigningCredentials TestSigningCredentials() =>
		new(
			new SymmetricSecurityKey(
				[.. "a-very-long-test-signing-key-0123456789"u8]),
			SecurityAlgorithms.HmacSha256);

	private static IConfiguration ConfigurationWith(
		string issuer,
		string audience,
		string sectionName = "Jwt") =>
		new ConfigurationBuilder()
			.AddInMemoryCollection(
				new Dictionary<string, string?>
				{
					[$"{sectionName}:Issuer"] = issuer,
					[$"{sectionName}:Subject"] = "subject",
					[$"{sectionName}:Audience"] = audience,
				})
			.Build();

	[Fact]
	public async Task AddJwtAuthentication_AppliesSigningCredentialsToValidationParameters()
	{
		SigningCredentials signingCredentials = TestSigningCredentials();
		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			signingCredentials);

		await using ServiceProvider provider = services.BuildServiceProvider();
		JwtBearerOptions bearerOptions = provider
			.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
			.Get(JwtBearerDefaults.AuthenticationScheme);
		TokenValidationParameters validationParameters = bearerOptions.TokenValidationParameters;

		Assert.Same(signingCredentials.Key, validationParameters.IssuerSigningKey);
		Assert.Equal("https://issuer.test", validationParameters.ValidIssuer);
		Assert.Equal("audience.test", validationParameters.ValidAudience);
		Assert.True(validationParameters.ValidateIssuerSigningKey);
		Assert.Equal(TimeSpan.FromMinutes(5), validationParameters.ClockSkew);
	}

	[Fact]
	public async Task AddJwtAuthentication_BindsJwtOptionsFromConfiguration()
	{
		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();
		JwtOptions jwtOptions = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

		Assert.Equal("https://issuer.test", jwtOptions.Issuer);
		Assert.Equal("audience.test", jwtOptions.Audience);
	}

	[Fact]
	public async Task AddJwtAuthentication_KeepsAFactoryRegisteredEarlier()
	{
		var custom = new StubTokenFactory();
		var services = new ServiceCollection();
		services.AddSingleton<IJwtTokenFactory>(custom);
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();

		Assert.Same(custom, provider.GetRequiredService<IJwtTokenFactory>());
	}

	[Fact]
	public async Task AddJwtAuthentication_RegistersAFactorySigningWithTheSameKey()
	{
		// 3.x never put the credentials where JwtTokenFactory read them, so a
		// factory resolved from DI signed with null credentials (B1).
		SigningCredentials signingCredentials = TestSigningCredentials();
		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			signingCredentials);

		await using ServiceProvider provider = services.BuildServiceProvider();

		Assert.IsType<JwtTokenFactory>(
			provider.GetRequiredService<IJwtTokenFactory>());
		Assert.Same(
			signingCredentials,
			provider.GetRequiredService<JwtSigningCredentials>().Credentials);
	}

	[Fact]
	public async Task AddJwtAuthentication_RegistersJwtBearerScheme()
	{
		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();
		var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
		AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync(
			JwtBearerDefaults.AuthenticationScheme);

		Assert.NotNull(scheme);
		Assert.Equal(typeof(JwtBearerHandler), scheme.HandlerType);
	}

	[Fact]
	public void AddJwtAuthentication_ThrowsWhenSigningCredentialsAreNull()
	{
		var services = new ServiceCollection();

		// SigningCredentials cannot be bound from configuration, so a null
		// argument must fail fast rather than producing a null signing key later.
		Assert.Throws<ArgumentNullException>(() =>
			services.AddJwtAuthentication(
				ConfigurationWith("https://issuer.test", "audience.test"),
				null!));
	}

	[Fact]
	public async Task AddJwtAuthentication_UsesCustomSectionName()
	{
		IConfiguration configuration = ConfigurationWith(
			"https://custom.test",
			"audience.test",
			"Tokens");

		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			configuration,
			TestSigningCredentials(),
			"Tokens");

		await using ServiceProvider provider = services.BuildServiceProvider();
		JwtOptions jwtOptions = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

		Assert.Equal("https://custom.test", jwtOptions.Issuer);
	}

	[Fact]
	public async Task IssuedToken_AuthenticatesAndCarriesTheUserId()
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			TestSigningCredentials());
		builder.Services.AddAuthorization();

		await using WebApplication app = builder.Build();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapGet(
				"/me",
				(ClaimsPrincipal user) => user.GetUserIdAsGuid().ToString("D"))
			.RequireAuthorization();
		await app.StartAsync();

		var userId = Guid.NewGuid();
		var factory = app.Services.GetRequiredService<IJwtTokenFactory>();
		string token = factory.GenerateToken(
			factory.GenerateClaimsIdentity("user@example.com", userId));

		using HttpClient client = app.GetTestClient();
		using var request = new HttpRequestMessage(HttpMethod.Get, "/me");
		request.Headers.Authorization =
			new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
		using HttpResponseMessage response = await client.SendAsync(request);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(userId.ToString("D"), await response.Content.ReadAsStringAsync());
	}

	[Fact]
	public async Task RequestWithoutAToken_IsRejected()
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			TestSigningCredentials());
		builder.Services.AddAuthorization();

		await using WebApplication app = builder.Build();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapGet("/me", () => Results.Ok()).RequireAuthorization();
		await app.StartAsync();

		using HttpClient client = app.GetTestClient();
		using HttpResponseMessage response = await client.GetAsync("/me");

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}
