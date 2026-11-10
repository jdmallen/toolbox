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
	public async Task GetCompletionAsync_CancellationToken_IsPropagated()
	{
		var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		var handler =
			new StubHttpMessageHandler(_ => Task.FromCanceled<HttpResponseMessage>(cts.Token));

		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await Assert.ThrowsAsync<TaskCanceledException>(()
			=> client.GetCompletionAsync(Request, cts.Token));
	}

	[Fact]
	public async Task GetCompletionAsync_ErrorResponse_ThrowsHttpRequestException()
	{
		var handler = new StubHttpMessageHandler(Json("Unauthorized", HttpStatusCode.Unauthorized));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		var exception =
			await Assert.ThrowsAsync<HttpRequestException>(()
				=> client.GetCompletionAsync(Request));

		Assert.Contains("Azure OpenAI API request failed: Unauthorized", exception.Message);
	}

	[Fact]
	public async Task GetCompletionAsync_NoImages_SendsUserContentAsPlainString()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await client.GetCompletionAsync(Request);

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement content = body.RootElement.GetProperty("messages")[1].GetProperty("content");

		Assert.Equal(JsonValueKind.String, content.ValueKind);
		Assert.Equal("user prompt", content.GetString());
	}

	[Fact]
	public async Task GetCompletionAsync_Success_ReturnsContentAndSendsAzureShape()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "feat: add thing" } } ] }"""));

		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		string result = (await client.GetCompletionAsync(Request)).Text;

		Assert.Equal("feat: add thing", result);
		Assert.Equal("test-azure-key", handler.RequestHeaders["api-key"]);
		Assert.Contains(
			"/openai/deployments/gpt-4/chat/completions",
			handler.RequestUri?.ToString());
		Assert.Contains("api-version=", handler.RequestUri?.ToString());
		Assert.Contains("\"max_completion_tokens\":500", handler.RequestBody);
		Assert.Contains("\"role\":\"system\"", handler.RequestBody);
	}

	[Fact]
	public async Task GetCompletionAsync_SendsOnlyBudgetAndTemperature()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		await client.GetCompletionAsync(Request);

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement root = body.RootElement;

		// max_tokens is deprecated and rejected by reasoning models; top_p and the
		// penalties were only ever sent at their service defaults.
		Assert.Equal(500, root.GetProperty("max_completion_tokens").GetInt32());
		Assert.Equal(0.1, root.GetProperty("temperature").GetDouble());
		Assert.False(root.TryGetProperty("max_tokens", out _));
		Assert.False(root.TryGetProperty("top_p", out _));
		Assert.False(root.TryGetProperty("frequency_penalty", out _));
		Assert.False(root.TryGetProperty("presence_penalty", out _));
	}

	[Fact]
	public async Task GetCompletionAsync_ReasoningModel_OmitsTemperature()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "choices": [ { "message": { "content": "ok" } } ] }"""));
		var client = new AzureOpenAIChatClient(
			handler.CreateClient(),
			Options with { IsReasoningModel = true });

		await client.GetCompletionAsync(Request with { MaxTokens = 2048 });

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement root = body.RootElement;

		Assert.Equal(2048, root.GetProperty("max_completion_tokens").GetInt32());
		Assert.False(root.TryGetProperty("temperature", out _));
	}

	[Fact]
	public async Task GetCompletionAsync_WithImages_SendsImageUrlPartsBeforeText()
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

		await client.GetCompletionAsync(request);

		using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement[] parts =
		[
			.. body.RootElement.GetProperty("messages")[1].GetProperty("content").EnumerateArray(),
		];

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

	[Fact]
	public async Task GetCompletionAsync_ReportsUsageAndMetadata()
	{
		var handler = new StubHttpMessageHandler(
			Json(
				"""
				{
					"id": "chatcmpl-123",
					"model": "gpt-4o-2024-08-06",
					"choices": [
						{ "message": { "content": "ls" }, "finish_reason": "length" }
					],
					"usage": { "prompt_tokens": 50, "completion_tokens": 5, "total_tokens": 55 }
				}
				"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		CompletionResult result = await client.GetCompletionAsync(Request);

		Assert.Equal("ls", result.Text);
		Assert.Equal(new TokenUsage(50, 5), result.Usage);
		Assert.Equal("gpt-4o-2024-08-06", result.Model);
		Assert.Equal("chatcmpl-123", result.Id);
		Assert.Equal("length", result.StopReason);
	}

	[Fact]
	public async Task GetCompletionAsync_NoChoices_StillReportsUsage()
	{
		var handler = new StubHttpMessageHandler(
			Json(
				"""{ "choices": [], "usage": { "prompt_tokens": 50, "completion_tokens": 0 } }"""));
		var client = new AzureOpenAIChatClient(handler.CreateClient(), Options);

		CompletionResult result = await client.GetCompletionAsync(Request);

		Assert.Equal(string.Empty, result.Text);
		Assert.Equal(new TokenUsage(50, 0), result.Usage);
		Assert.Null(result.StopReason);
	}
}
