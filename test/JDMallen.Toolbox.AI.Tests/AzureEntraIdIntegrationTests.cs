namespace JDMallen.Toolbox.AI.Tests;

/// <summary>
/// End-to-end check against a real Azure OpenAI deployment through the real
/// Azure CLI: <c>az</c> process, token cache, bearer header, and response.
/// Skipped unless both variables are set, so CI and a fresh clone never run it.
/// It spends a few hundred tokens on the configured deployment.
/// </summary>
/// <remarks>
/// <code>
/// az login --scope https://ai.azure.com/.default
/// TOOLBOX_AI_ENTRA_ENDPOINT=https://foundry-jdm-01.openai.azure.com \
/// TOOLBOX_AI_ENTRA_DEPLOYMENT=gpt-6-luna \
///   dotnet test test/JDMallen.Toolbox.AI.Tests --filter AzureEntraIdIntegrationTests
/// </code>
/// </remarks>
public class AzureEntraIdIntegrationTests
{
	private const string ENDPOINT_VARIABLE = "TOOLBOX_AI_ENTRA_ENDPOINT";
	private const string DEPLOYMENT_VARIABLE = "TOOLBOX_AI_ENTRA_DEPLOYMENT";
	private const string TENANT_VARIABLE = "TOOLBOX_AI_ENTRA_TENANT";
	private const string TOKEN_ONLY_VARIABLE = "TOOLBOX_AI_ENTRA_TOKEN_TEST";

	/// <summary>
	/// The real <c>az</c> process and parsing, without calling a model (free).
	/// Opt in with <c>TOOLBOX_AI_ENTRA_TOKEN_TEST=1</c> after <c>az login</c>.
	/// </summary>
	[SkippableFact]
	public async Task AzureCliTokenProvider_RealAz_ReturnsUnexpiredToken()
	{
		Skip.IfNot(
			Environment.GetEnvironmentVariable(TOKEN_ONLY_VARIABLE) == "1",
			$"Set {TOKEN_ONLY_VARIABLE}=1 (after az login) to run.");

		var provider = new AzureCliTokenProvider(
			new AzureCliTokenProviderOptions
			{
				TenantId = Environment.GetEnvironmentVariable(TENANT_VARIABLE),
			});

		BearerToken token = await provider.GetTokenAsync();

		Assert.False(string.IsNullOrEmpty(token.Value));
		Assert.True(token.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(5));
	}

	[SkippableFact]
	public async Task GetCompletionAsync_RealAzureCliToken_Succeeds()
	{
		string? endpoint = Environment.GetEnvironmentVariable(ENDPOINT_VARIABLE);
		string? deployment = Environment.GetEnvironmentVariable(DEPLOYMENT_VARIABLE);
		Skip.If(
			string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment),
			$"Set {ENDPOINT_VARIABLE} and {DEPLOYMENT_VARIABLE} to run.");

		var tokens = new CachingBearerTokenProvider(
			new AzureCliTokenProvider(
				new AzureCliTokenProviderOptions
				{
					TenantId = Environment.GetEnvironmentVariable(TENANT_VARIABLE),
				}));

		// IsReasoningModel omits temperature, which every deployment accepts, and
		// the generous budget leaves room for hidden reasoning.
		using var httpClient = new HttpClient();
		httpClient.Timeout = TimeSpan.FromMinutes(2);
		var client = new AzureOpenAIChatClient(
			httpClient,
			AzureOpenAIClientOptions.ForEntraId(endpoint, deployment, tokens) with
			{
				IsReasoningModel = true,
			});

		CompletionResult result = await client.GetCompletionAsync(
			new CompletionRequest("Reply with one word.", "Say ok.", 512));

		Assert.NotNull(result.Usage);
		Assert.False(string.IsNullOrEmpty(result.Model));
	}
}
