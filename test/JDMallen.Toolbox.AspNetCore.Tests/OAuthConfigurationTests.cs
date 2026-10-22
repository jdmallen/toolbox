using JDMallen.Toolbox.AspNetCore.Options;
using Microsoft.Extensions.Configuration;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="OAuthConfiguration" /> binds from configuration.
/// </summary>
public class OAuthConfigurationTests
{
	[Fact]
	public void Scopes_BindFromConfiguration()
	{
		// 3.x declared GoogleScopes get-only, so binding never populated it (B3).
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(
				new Dictionary<string, string?>
				{
					["OAuth:GoogleScopes:0"] = "openid",
					["OAuth:GoogleScopes:1"] = "profile",
					["OAuth:GitHubScopes:0"] = "read:user",
				})
			.Build();

		var options =
			configuration.GetSection("OAuth").Get<OAuthConfiguration>();

		Assert.NotNull(options);
		Assert.Equal(["openid", "profile"], options.GoogleScopes);
		Assert.Equal(["read:user"], options.GitHubScopes);
	}
}
