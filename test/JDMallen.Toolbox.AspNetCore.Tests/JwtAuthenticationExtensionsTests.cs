using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="JwtAuthenticationExtensions.AddJwtAuthentication" />
/// registers the JWT bearer scheme, binds <see cref="JwtOptions" /> from
/// configuration, applies the supplied signing credentials to the token
/// validation parameters, and fails fast when no credentials are available.
/// </summary>
public class JwtAuthenticationExtensionsTests
{
	private static SigningCredentials TestSigningCredentials() =>
		new(
			new SymmetricSecurityKey(
				"a-very-long-test-signing-key-0123456789"u8.ToArray()),
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
			signingCredentials: signingCredentials);

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
			signingCredentials: TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();
		JwtOptions jwtOptions = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

		Assert.Equal("https://issuer.test", jwtOptions.Issuer);
		Assert.Equal("audience.test", jwtOptions.Audience);
	}

	[Fact]
	public async Task AddJwtAuthentication_RegistersJwtBearerScheme()
	{
		var services = new ServiceCollection();
		services.AddJwtAuthentication(
			ConfigurationWith("https://issuer.test", "audience.test"),
			signingCredentials: TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();
		var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
		AuthenticationScheme? scheme = await schemeProvider.GetSchemeAsync(
			JwtBearerDefaults.AuthenticationScheme);

		Assert.NotNull(scheme);
		Assert.Equal(typeof(JwtBearerHandler), scheme.HandlerType);
	}

	[Fact]
	public void AddJwtAuthentication_ThrowsWhenSigningCredentialsAreMissing()
	{
		var services = new ServiceCollection();

		// SigningCredentials cannot be bound from configuration, so omitting the
		// argument must fail fast rather than producing a null signing key later.
		Assert.Throws<InvalidOperationException>(() =>
			services.AddJwtAuthentication(
				ConfigurationWith("https://issuer.test", "audience.test")));
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
			"Tokens",
			TestSigningCredentials());

		await using ServiceProvider provider = services.BuildServiceProvider();
		JwtOptions jwtOptions = provider.GetRequiredService<IOptions<JwtOptions>>().Value;

		Assert.Equal("https://custom.test", jwtOptions.Issuer);
	}
}
