using System.Net;
using System.Text;
using System.Text.Json;

namespace JDMallen.Toolbox.AI.Tests;

public class AnthropicChatClientTests
{
	private static readonly AnthropicClientOptions Options = new(
		"test-anthropic-key",
		"claude-haiku-4-5-20251001");

	private static readonly CompletionRequest Request = new("system", "user prompt", 500);

	private static HttpResponseMessage Json(string content, HttpStatusCode code = HttpStatusCode.OK)
		=>
			new(code) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

	[Fact]
	public async Task CompleteAsync_ErrorResponse_ThrowsHttpRequestException()
	{
		var handler = new StubHttpMessageHandler(Json("bad request", HttpStatusCode.BadRequest));
		var client = new AnthropicChatClient(handler.CreateClient(), Options);

		var exception
			= await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(Request));

		Assert.Contains("Anthropic API request failed: BadRequest", exception.Message);
	}

	[Fact]
	public async Task CompleteAsync_SkipsNonTextBlocks()
	{
		var handler = new StubHttpMessageHandler(
			Json(
				"""
				{ "content": [
					{ "type": "thinking", "thinking": "hmm" },
					{ "type": "text", "text": "fix: the bug" }
				] }
				"""));

		var client = new AnthropicChatClient(handler.CreateClient(), Options);

		string result = await client.CompleteAsync(Request);

		Assert.Equal("fix: the bug", result);
	}

	[Fact]
	public async Task CompleteAsync_Success_ReturnsTextAndSendsAnthropicShape()
	{
		var handler = new StubHttpMessageHandler(
			Json("""{ "content": [ { "type": "text", "text": "feat: add thing\n\nDetail." } ] }"""));

		var client = new AnthropicChatClient(handler.CreateClient(), Options);

		string result = await client.CompleteAsync(Request);

		Assert.Equal("feat: add thing\n\nDetail.", result);
		Assert.Equal("test-anthropic-key", handler.RequestHeaders["x-api-key"]);
		Assert.Equal("2023-06-01", handler.RequestHeaders["anthropic-version"]);
		Assert.Equal("https://api.anthropic.com/v1/messages", handler.RequestUri?.ToString());
		Assert.Contains("\"model\":\"claude-haiku-4-5-20251001\"", handler.RequestBody);
		Assert.Contains("\"system\":\"system\"", handler.RequestBody);
		Assert.Contains("\"max_tokens\":500", handler.RequestBody);
	}

	[Fact]
	public async Task CompleteAsync_NoImages_SendsUserContentAsPlainString()
	{
		var handler = new StubHttpMessageHandler(Json("""{ "content": [ { "type": "text", "text": "ok" } ] }"""));
		var client = new AnthropicChatClient(handler.CreateClient(), Options);

		await client.CompleteAsync(Request);

		using var body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement content = body.RootElement.GetProperty("messages")[0].GetProperty("content");

		Assert.Equal(JsonValueKind.String, content.ValueKind);
		Assert.Equal("user prompt", content.GetString());
	}

	[Fact]
	public async Task CompleteAsync_WithImages_SendsBase64ImageBlocksBeforeText()
	{
		var handler = new StubHttpMessageHandler(Json("""{ "content": [ { "type": "text", "text": "ok" } ] }"""));
		var client = new AnthropicChatClient(handler.CreateClient(), Options);
		CompletionRequest request = Request with
		{
			Images = [new ImageInput(new byte[] { 1, 2, 3 }, "image/jpeg")],
		};

		await client.CompleteAsync(request);

		using var body = JsonDocument.Parse(handler.RequestBody!);
		JsonElement[] blocks = [.. body.RootElement.GetProperty("messages")[0].GetProperty("content").EnumerateArray()];

		Assert.Equal(2, blocks.Length);
		Assert.Equal("image", blocks[0].GetProperty("type").GetString());
		JsonElement source = blocks[0].GetProperty("source");
		Assert.Equal("base64", source.GetProperty("type").GetString());
		Assert.Equal("image/jpeg", source.GetProperty("media_type").GetString());
		Assert.Equal("AQID", source.GetProperty("data").GetString());
		Assert.False(blocks[0].TryGetProperty("text", out _));
		Assert.Equal("text", blocks[1].GetProperty("type").GetString());
		Assert.Equal("user prompt", blocks[1].GetProperty("text").GetString());
		Assert.False(blocks[1].TryGetProperty("source", out _));
	}
}
