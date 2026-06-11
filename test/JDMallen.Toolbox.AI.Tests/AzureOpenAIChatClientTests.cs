using System.Net;
using System.Text;

namespace JDMallen.Toolbox.AI.Tests;

public class AzureOpenAIChatClientTests
{
	private static readonly AzureOpenAIClientOptions Options = new(
		ApiKey: "test-azure-key",
		Endpoint: "https://test.openai.azure.com",
		Deployment: "gpt-4");

	private static readonly CompletionRequest Request = new("system", "user prompt", 500);

	private static HttpResponseMessage Json(string content, HttpStatusCode code = HttpStatusCode.OK)
		=>
			new(code) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

	[Fact]
	public async Task CompleteAsync_CancellationToken_IsPropagated()
	{
		var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		var handler = new StubHttpMessageHandler(
			_ => Task.FromCanceled<HttpResponseMessage>(cts.Token));

		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await Assert.ThrowsAsync<TaskCanceledException>(() => client.CompleteAsync(Request, cts.Token));
	}

	[Fact]
	public async Task CompleteAsync_ErrorResponse_ThrowsHttpRequestException()
	{
		var handler = new StubHttpMessageHandler(Json("Unauthorized", HttpStatusCode.Unauthorized));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		var exception
			= await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(Request));

		Assert.Contains("Azure OpenAI API request failed: Unauthorized", exception.Message);
	}

	[Fact]
	public async Task CompleteAsync_Success_ReturnsContentAndSendsAzureShape()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "feat: add thing" } } ] }"""));

		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		string result = await client.CompleteAsync(Request);

		Assert.Equal("feat: add thing", result);
		Assert.Equal("test-azure-key", handler.RequestHeaders["api-key"]);
		Assert.Contains("/openai/deployments/gpt-4/chat/completions", handler.RequestUri?.ToString());
		Assert.Contains("api-version=", handler.RequestUri?.ToString());
		Assert.Contains("\"max_tokens\":500", handler.RequestBody);
		Assert.Contains("\"role\":\"system\"", handler.RequestBody);
	}
}
