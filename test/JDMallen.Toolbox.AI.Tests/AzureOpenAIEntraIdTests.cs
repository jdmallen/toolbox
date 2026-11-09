using System.Net;
using System.Text;

namespace JDMallen.Toolbox.AI.Tests;

public class AzureOpenAIEntraIdTests
{
	private const string FAKE_TOKEN = "eyJ0eXAiOiJKV1QifQ.fake-token-value";

	private static readonly CompletionRequest Request = new("system", "user prompt", 500);

	private sealed class StaticTokenProvider : IBearerTokenProvider
	{
		public int Calls { get; private set; }

		public ValueTask<BearerToken> GetTokenAsync(CancellationToken cancellationToken = default)
		{
			Calls++;

			return ValueTask.FromResult(
				new BearerToken(FAKE_TOKEN, DateTimeOffset.UtcNow.AddHours(1)));
		}
	}

	private static HttpResponseMessage Json(string content, HttpStatusCode code = HttpStatusCode.OK)
		=> new(code) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

	private static StubHttpMessageHandler OkHandler()
		=> new(Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));

	[Fact]
	public async Task GetCompletionAsync_WithTokenProvider_SendsBearerAndNoApiKey()
	{
		StubHttpMessageHandler handler = OkHandler();
		var tokens = new StaticTokenProvider();
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			AzureOpenAIClientOptions.ForEntraId("https://test.openai.azure.com", "gpt-6-luna", tokens));

		CompletionResult result = await client.GetCompletionAsync(Request);

		Assert.Equal("ok", result.Text);
		Assert.Equal($"Bearer {FAKE_TOKEN}", handler.RequestHeaders["Authorization"]);
		Assert.False(handler.RequestHeaders.ContainsKey("api-key"));
		Assert.Equal(1, tokens.Calls);
	}

	[Fact]
	public async Task GetCompletionAsync_TokenProviderSetWithApiKey_ProviderWins()
	{
		StubHttpMessageHandler handler = OkHandler();
		var options = new AzureOpenAIClientOptions("leftover-key", "https://test.openai.azure.com", "d")
		{
			TokenProvider = new StaticTokenProvider(),
		};
		var client = new AzureOpenAIChatClient(handler.CreateClient(), options);

		await client.GetCompletionAsync(Request);

		Assert.False(handler.RequestHeaders.ContainsKey("api-key"));
		Assert.StartsWith("Bearer ", handler.RequestHeaders["Authorization"]);
	}

	[Fact]
	public void ForEntraId_SetsProviderAndNoKey()
	{
		var tokens = new StaticTokenProvider();

		AzureOpenAIClientOptions options =
			AzureOpenAIClientOptions.ForEntraId("https://x.openai.azure.com", "d", tokens);

		Assert.Same(tokens, options.TokenProvider);
		Assert.Equal(string.Empty, options.ApiKey);
		Assert.Equal("https://x.openai.azure.com", options.Endpoint);
		Assert.Equal("d", options.Deployment);
	}

	[Theory]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	public async Task GetCompletionAsync_AuthFailureWithToken_AddsHintWithoutToken(
		HttpStatusCode code)
	{
		var handler = new StubHttpMessageHandler(Json("""{"error":"PermissionDenied"}""", code));
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			AzureOpenAIClientOptions.ForEntraId(
				"https://test.openai.azure.com",
				"d",
				new StaticTokenProvider()));

		var exception =
			await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCompletionAsync(Request));

		Assert.Contains("Cognitive Services OpenAI User", exception.Message);
		Assert.Contains("PermissionDenied", exception.Message);
		Assert.DoesNotContain(FAKE_TOKEN, exception.Message);
		Assert.Equal(code, exception.StatusCode);
	}

	[Fact]
	public async Task GetCompletionAsync_AuthFailureWithApiKey_HasNoEntraHint()
	{
		var handler = new StubHttpMessageHandler(Json("Unauthorized", HttpStatusCode.Unauthorized));
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			new AzureOpenAIClientOptions("key", "https://test.openai.azure.com", "d"));

		var exception =
			await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCompletionAsync(Request));

		Assert.DoesNotContain("Entra ID", exception.Message);
		Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
	}

	[Fact]
	public async Task GetCompletionAsync_OtherFailureWithToken_HasNoHint()
	{
		var handler = new StubHttpMessageHandler(Json("busy", HttpStatusCode.TooManyRequests));
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			AzureOpenAIClientOptions.ForEntraId(
				"https://test.openai.azure.com",
				"d",
				new StaticTokenProvider()));

		var exception =
			await Assert.ThrowsAsync<HttpRequestException>(() => client.GetCompletionAsync(Request));

		Assert.DoesNotContain("Entra ID", exception.Message);
	}

	[Fact]
	public async Task GetCompletionAsync_TokenOverPlainHttp_IsRefusedBeforeFetching()
	{
		StubHttpMessageHandler handler = OkHandler();
		var tokens = new StaticTokenProvider();
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			AzureOpenAIClientOptions.ForEntraId("http://test.openai.azure.com", "d", tokens));

		var exception =
			await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetCompletionAsync(Request));

		Assert.Contains("https", exception.Message);
		Assert.Equal(0, tokens.Calls);
		Assert.Null(handler.RequestUri);
	}

	[Fact]
	public async Task GetCompletionAsync_TokenOverLoopbackHttp_IsAllowed()
	{
		StubHttpMessageHandler handler = OkHandler();
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			AzureOpenAIClientOptions.ForEntraId("http://localhost:8080", "d", new StaticTokenProvider()));

		await client.GetCompletionAsync(Request);

		Assert.StartsWith("Bearer ", handler.RequestHeaders["Authorization"]);
	}

	[Fact]
	public async Task GetCompletionAsync_ApiKeyOverPlainHttp_IsUnchanged()
	{
		StubHttpMessageHandler handler = OkHandler();
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			new AzureOpenAIClientOptions("key", "http://test.openai.azure.com", "d"));

		await client.GetCompletionAsync(Request);

		Assert.Equal("key", handler.RequestHeaders["api-key"]);
	}

	[Theory]
	[InlineData("https://x.openai.azure.com", "https://x.openai.azure.com")]
	[InlineData("https://x.openai.azure.com/", "https://x.openai.azure.com")]
	[InlineData("https://x.openai.azure.com/openai", "https://x.openai.azure.com")]
	[InlineData("https://x.openai.azure.com/openai/", "https://x.openai.azure.com")]
	[InlineData("https://x.openai.azure.com/openai/v1/", "https://x.openai.azure.com")]
	[InlineData("https://x.services.ai.azure.com/OpenAI/V1", "https://x.services.ai.azure.com")]
	[InlineData("https://apim.example.com/aoai", "https://apim.example.com/aoai")]
	[InlineData("https://apim.example.com/aoai/openai/v1", "https://apim.example.com/aoai")]
	public void NormalizeEndpoint_StripsOnlyPastedApiRoot(string endpoint, string expected)
	{
		Assert.Equal(expected, AzureOpenAIChatClient.NormalizeEndpoint(endpoint));
	}

	[Fact]
	public async Task GetCompletionAsync_PastedV1Endpoint_BuildsDeploymentUrl()
	{
		StubHttpMessageHandler handler = OkHandler();
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			new AzureOpenAIClientOptions("key", "https://x.openai.azure.com/openai/v1/", "gpt-6-luna"));

		await client.GetCompletionAsync(Request);

		Assert.Equal(
			"https://x.openai.azure.com/openai/deployments/gpt-6-luna/chat/completions?api-version=2024-10-21",
			handler.RequestUri?.ToString());
	}
}
