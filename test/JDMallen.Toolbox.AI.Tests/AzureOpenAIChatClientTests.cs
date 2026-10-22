using System.Net;
using System.Text;
using System.Text.Json;

namespace JDMallen.Toolbox.AI.Tests;

public class AzureOpenAIChatClientTests
{
	private static readonly AzureOpenAIClientOptions Options = new(
		"test-azure-key",
		"https://test.openai.azure.com",
		"gpt-4");

	private static readonly CompletionRequest Request = new("system", "user prompt", 500);

	private static HttpResponseMessage Json(string content, HttpStatusCode code = HttpStatusCode.OK)
		=> new(code) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

	[Fact]
	public async Task CompleteAsync_CancellationToken_IsPropagated()
	{
		var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		var handler =
			new StubHttpMessageHandler(_ => Task.FromCanceled<HttpResponseMessage>(cts.Token));

		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await Assert.ThrowsAsync<TaskCanceledException>(() => client.CompleteAsync(Request, cts.Token));
	}

	[Fact]
	public async Task CompleteAsync_ErrorResponse_ThrowsHttpRequestException()
	{
		var handler = new StubHttpMessageHandler(Json("Unauthorized", HttpStatusCode.Unauthorized));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		var exception =
			await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(Request));

		Assert.Contains("Azure OpenAI API request failed: Unauthorized", exception.Message);
	}

	[Fact]
	public async Task CompleteAsync_NoImages_SendsUserContentAsPlainString()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await client.CompleteAsync(Request);

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement content = body.RootElement.GetProperty("messages")[1].GetProperty("content");

		Assert.Equal(JsonValueKind.String, content.ValueKind);
		Assert.Equal("user prompt", content.GetString());
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

	[Fact]
	public async Task CompleteAsync_WithImages_SendsImageUrlPartsBeforeText()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);
		CompletionRequest request = Request with
		{
			Images =
			[
				new ImageInput(new byte[] { 1, 2, 3 }, "image/jpeg"),
				new ImageInput(new byte[] { 4, 5, 6 }, "image/png"),
			],
		};

		await client.CompleteAsync(request);

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement[] parts =
			[.. body.RootElement.GetProperty("messages")[1].GetProperty("content").EnumerateArray()];

		Assert.Equal(3, parts.Length);
		Assert.Equal("image_url", parts[0].GetProperty("type").GetString());
		Assert.Equal(
			"data:image/jpeg;base64,AQID",
			parts[0].GetProperty("image_url").GetProperty("url").GetString());
		Assert.Equal(
			"data:image/png;base64,BAUG",
			parts[1].GetProperty("image_url").GetProperty("url").GetString());
		Assert.False(parts[0].TryGetProperty("text", out _));
		Assert.Equal("text", parts[2].GetProperty("type").GetString());
		Assert.Equal("user prompt", parts[2].GetProperty("text").GetString());
		Assert.False(parts[2].TryGetProperty("image_url", out _));
	}
}
