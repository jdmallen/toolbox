using System.ComponentModel;

namespace JDMallen.Toolbox.AI.Tests;

public class AzureCliTokenProviderTests
{
	private const string FAKE_TOKEN = "eyJ0eXAiOiJKV1QifQ.fake-token-value";

	private const string SUCCESS_JSON =
		$$"""
		  {
		  	"accessToken": "{{FAKE_TOKEN}}",
		  	"expiresOn": "2026-11-09 16:20:31.000000",
		  	"expires_on": 1794259231,
		  	"subscription": "00000000-0000-0000-0000-000000000000",
		  	"tenant": "11111111-1111-1111-1111-111111111111",
		  	"tokenType": "Bearer"
		  }
		  """;

	private static AzureCliTokenProvider Provider(
		FakeProcessRunner runner,
		AzureCliTokenProviderOptions? options = null,
		bool isWindows = false)
		=> new(options ?? new AzureCliTokenProviderOptions(), runner, isWindows);

	[Fact]
	public async Task GetTokenAsync_Success_ParsesTokenAndEpochExpiry()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);

		BearerToken token = await Provider(runner).GetTokenAsync();

		Assert.Equal(FAKE_TOKEN, token.Value);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1794259231), token.ExpiresOn);
	}

	[Fact]
	public async Task GetTokenAsync_RunsAzWithDefaultScopeAndJsonOutput()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);

		await Provider(runner).GetTokenAsync();

		Assert.Equal("az", runner.FileName);
		Assert.Equal(
			[
				"account", "get-access-token",
				"--scope", AzureOpenAIScopes.DEFAULT,
				"--output", "json",
			],
			runner.Arguments);
		Assert.Equal(TimeSpan.FromSeconds(30), runner.Timeout);
	}

	[Fact]
	public async Task GetTokenAsync_WithTenantAndScope_PassesBoth()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);
		var options = new AzureCliTokenProviderOptions
		{
			Scope = AzureOpenAIScopes.COGNITIVE_SERVICES,
			TenantId = "contoso.onmicrosoft.com",
			Timeout = TimeSpan.FromSeconds(5),
		};

		await Provider(runner, options).GetTokenAsync();

		Assert.Equal(
			[
				"account", "get-access-token",
				"--scope", AzureOpenAIScopes.COGNITIVE_SERVICES,
				"--tenant", "contoso.onmicrosoft.com",
				"--output", "json",
			],
			runner.Arguments);
		Assert.Equal(TimeSpan.FromSeconds(5), runner.Timeout);
	}

	[Fact]
	public async Task GetTokenAsync_CustomPath_RunsThatExecutable()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);
		var options = new AzureCliTokenProviderOptions { AzExecutablePath = "/opt/az/bin/az" };

		await Provider(runner, options).GetTokenAsync();

		Assert.Equal("/opt/az/bin/az", runner.FileName);
		Assert.Equal("account", runner.Arguments[0]);
	}

	[Fact]
	public async Task GetTokenAsync_OnWindows_RunsAzThroughCmd()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);

		await Provider(runner, isWindows: true).GetTokenAsync();

		Assert.Equal("cmd.exe", runner.FileName);
		Assert.Equal(["/d", "/c", "az", "account", "get-access-token"], runner.Arguments.Take(5));
	}

	[Fact]
	public async Task GetTokenAsync_OnWindowsWithExePath_RunsItDirectly()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, SUCCESS_JSON);
		var options = new AzureCliTokenProviderOptions { AzExecutablePath = @"C:\tools\az.exe" };

		await Provider(runner, options, true).GetTokenAsync();

		Assert.Equal(@"C:\tools\az.exe", runner.FileName);
	}

	[Fact]
	public async Task GetTokenAsync_ExpiresOnAsString_IsAccepted()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			0,
			"""{ "accessToken": "t", "expires_on": "1794259231" }""");

		BearerToken token = await Provider(runner).GetTokenAsync();

		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1794259231), token.ExpiresOn);
	}

	[Fact]
	public async Task GetTokenAsync_NoExpiresOn_AsksForNewerAz()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			0,
			"""{ "accessToken": "t", "expiresOn": "2026-11-09 16:20:31.000000" }""");

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains("2.54", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_NoAccessToken_Throws()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			0,
			"""{ "expires_on": 1794259231 }""");

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains("no access token", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_UnparseableOutput_DoesNotEchoIt()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(0, $"not json {FAKE_TOKEN}");

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.DoesNotContain(FAKE_TOKEN, exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_NotSignedIn_SuggestsAzLogin()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			1,
			standardError: "ERROR: Please run 'az login' to setup account.\n");

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains($"az login --scope {AzureOpenAIScopes.DEFAULT}", exception.Message);
		Assert.Contains("Please run 'az login'", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_AadstsErrorWithTenant_SuggestsTenantLogin()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			1,
			standardError: "ERROR: AADSTS50076: Due to a configuration change ...");
		var options = new AzureCliTokenProviderOptions
		{
			TenantId = "11111111-1111-1111-1111-111111111111",
		};

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner, options).GetTokenAsync().AsTask());

		Assert.Contains(
			"az login --tenant 11111111-1111-1111-1111-111111111111 --scope",
			exception.Message);
		Assert.Contains("AADSTS50076", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_OtherFailure_ReportsExitCodeAndStderr()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			2,
			standardError: "ERROR: something odd");

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains("exited with code 2", exception.Message);
		Assert.Contains("something odd", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_LongStderr_IsTruncated()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			1,
			standardError: new string('x', 5000));

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.True(exception.Message.Length < 1300);
	}

	[Fact]
	public async Task GetTokenAsync_AzMissing_SaysToInstall()
	{
		FakeProcessRunner runner =
			FakeProcessRunner.Throwing(new Win32Exception(2, "No such file"));

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains("not found on PATH", exception.Message);
		Assert.IsType<Win32Exception>(exception.InnerException);
	}

	[Fact]
	public async Task GetTokenAsync_AzMissingOnWindows_SaysToInstall()
	{
		FakeProcessRunner runner = FakeProcessRunner.Returning(
			9009,
			standardError: "'az' is not recognized as an internal or external command");

		var exception = await Assert.ThrowsAsync<AzureCliCredentialException>(()
			=> Provider(runner, isWindows: true).GetTokenAsync().AsTask());

		Assert.Contains("not found on PATH", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_Timeout_ReportsIt()
	{
		FakeProcessRunner runner = FakeProcessRunner.Throwing(new TimeoutException("slow"));

		var exception =
			await Assert.ThrowsAsync<AzureCliCredentialException>(()
				=> Provider(runner).GetTokenAsync().AsTask());

		Assert.Contains("within 30 seconds", exception.Message);
	}

	[Fact]
	public async Task GetTokenAsync_Cancelled_PropagatesCancellation()
	{
		FakeProcessRunner runner = FakeProcessRunner.Throwing(new OperationCanceledException());

		await Assert.ThrowsAsync<OperationCanceledException>(()
			=> Provider(runner).GetTokenAsync().AsTask());
	}

	[Theory]
	[InlineData("ai.azure.com/.default")]
	[InlineData("http://ai.azure.com/.default")]
	[InlineData("https://ai.azure.com/.default & calc")]
	[InlineData("--debug")]
	[InlineData("")]
	public void Constructor_InvalidScope_Throws(string scope)
	{
		var options = new AzureCliTokenProviderOptions { Scope = scope };

		Assert.Throws<ArgumentException>(() => Provider(FakeProcessRunner.Returning(0), options));
	}

	[Theory]
	[InlineData("--subscription")]
	[InlineData("contoso & calc")]
	[InlineData("tenant\"quote")]
	public void Constructor_InvalidTenant_Throws(string tenantId)
	{
		var options = new AzureCliTokenProviderOptions { TenantId = tenantId };

		Assert.Throws<ArgumentException>(() => Provider(FakeProcessRunner.Returning(0), options));
	}

	[Fact]
	public void Constructor_NonPositiveTimeout_Throws()
	{
		var options = new AzureCliTokenProviderOptions { Timeout = TimeSpan.Zero };

		Assert.Throws<ArgumentException>(() => Provider(FakeProcessRunner.Returning(0), options));
	}

	[Fact]
	public void BearerToken_ToString_HidesValue()
	{
		var token = new BearerToken(FAKE_TOKEN, DateTimeOffset.UnixEpoch);

		Assert.DoesNotContain(FAKE_TOKEN, token.ToString());
	}
}
