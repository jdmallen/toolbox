using System.Net;
using System.Text;

namespace JDMallen.Toolbox.AI.Tests;

public class OpenAICompatibleChatClientTests
{
	private static readonly OpenAICompatibleClientOptions Options = new(
		"http://10.10.0.20:8080/v1",
		"thinkingcap-27b-q4km");

	private static readonly CompletionRequest Request = new("system", "user prompt", 500);

	private static HttpResponseMessage Json(string content, HttpStatusCode code = HttpStatusCode.OK)
		=>
			new(code) { Content = new StringContent(content, Encoding.UTF8, "application/json") };

	private static HttpResponseMessage Completion(string content)
		=> Json($$"""{ "choices": [ { "message": { "content": {{content}} } } ] }""");

	[Fact]
	public async Task CompleteAsync_CancellationToken_IsPropagated()
	{
		var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		var handler
			= new StubHttpMessageHandler(_ => Task.FromCanceled<HttpResponseMessage>(cts.Token));

		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		await Assert.ThrowsAsync<TaskCanceledException>(() => client.CompleteAsync(Request, cts.Token));
	}

	[Fact]
	public async Task CompleteAsync_ErrorResponse_ThrowsHttpRequestException()
	{
		var handler = new StubHttpMessageHandler(Json("model not found", HttpStatusCode.NotFound));
		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		var exception
			= await Assert.ThrowsAsync<HttpRequestException>(() => client.CompleteAsync(Request));

		Assert.Contains("OpenAI-compatible API request failed: NotFound", exception.Message);
	}

	[Fact]
	public async Task CompleteAsync_Success_ReturnsContentAndSendsOpenAIShape()
	{
		var handler = new StubHttpMessageHandler(Completion("\"ls -la\""));
		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		string result = await client.CompleteAsync(Request);

		Assert.Equal("ls -la", result);
		Assert.Equal("http://10.10.0.20:8080/v1/chat/completions", handler.RequestUri?.ToString());
		Assert.Contains("\"model\":\"thinkingcap-27b-q4km\"", handler.RequestBody);
		Assert.Contains("\"max_tokens\":500", handler.RequestBody);
		Assert.Contains("\"role\":\"system\"", handler.RequestBody);
		Assert.Contains("\"stream\":false", handler.RequestBody);
	}

	[Fact]
	public async Task CompleteAsync_TrailingSlashInBaseUrl_DoesNotDoubleUpPath()
	{
		var handler = new StubHttpMessageHandler(Completion("\"ok\""));
		var client = new OpenAICompatibleChatClient(
			handler.CreateClient(),
			Options with { BaseUrl = "http://10.10.0.20:8080/v1/" });

		await client.CompleteAsync(Request);

		Assert.Equal("http://10.10.0.20:8080/v1/chat/completions", handler.RequestUri?.ToString());
	}

	[Fact]
	public async Task CompleteAsync_ApiKeySupplied_SendsBearerHeader()
	{
		var handler = new StubHttpMessageHandler(Completion("\"ok\""));
		var client = new OpenAICompatibleChatClient(
			handler.CreateClient(),
			Options with { ApiKey = "sk-test" });

		await client.CompleteAsync(Request);

		Assert.Equal("Bearer sk-test", handler.RequestHeaders["Authorization"]);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public async Task CompleteAsync_NoApiKey_OmitsAuthorizationHeader(string? apiKey)
	{
		var handler = new StubHttpMessageHandler(Completion("\"ok\""));
		var client = new OpenAICompatibleChatClient(
			handler.CreateClient(),
			Options with { ApiKey = apiKey });

		await client.CompleteAsync(Request);

		Assert.DoesNotContain("Authorization", handler.RequestHeaders.Keys);
	}

	[Fact]
	public async Task CompleteAsync_NoChoices_ReturnsEmpty()
	{
		var handler = new StubHttpMessageHandler(Json("""{ "choices": [] }"""));
		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		Assert.Equal(string.Empty, await client.CompleteAsync(Request));
	}

	[Theory]
	// A closed reasoning block is removed, whatever the tag name.
	[InlineData("<think>hmm, ls then</think>ls -la", "ls -la")]
	[InlineData("<thinking>hmm</thinking>\n\nls -la", "ls -la")]
	[InlineData("<reasoning>hmm</reasoning>ls -la", "ls -la")]
	[InlineData("<THINK>hmm</THINK>ls -la", "ls -la")]
	// Blocks may be interleaved with, or trail, the answer.
	[InlineData("<think>a</think>ls<think>b</think> -la", "ls -la")]
	[InlineData("ls -la<think>done</think>", "ls -la")]
	// Content without reasoning is passed through untouched, including an
	// unclosed tag that is plainly part of the answer rather than a stray thought.
	[InlineData("grep -r '<think>' .", "grep -r '<think>' .")]
	[InlineData("echo '<reasoning>' > f.xml", "echo '<reasoning>' > f.xml")]
	[InlineData("", "")]
	[InlineData("   ", "")]
	public async Task CompleteAsync_ReasoningBlocks_AreStripped(string content, string expected)
	{
		var handler = new StubHttpMessageHandler(
			Completion(System.Text.Json.JsonSerializer.Serialize(content)));

		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		Assert.Equal(expected, await client.CompleteAsync(Request));
	}

	[Theory]
	[InlineData("<think>the user wants to list files, so I should")]
	[InlineData("\n\n  <think>the user wants to list files, so I should")]
	public async Task CompleteAsync_LeadingUnclosedReasoningBlock_DiscardsTruncatedThought(
		string content)
	{
		// The model hit max_tokens mid-thought; returning the reasoning as if it
		// were the answer would be worse than returning nothing.
		var handler = new StubHttpMessageHandler(
			Completion(System.Text.Json.JsonSerializer.Serialize(content)));

		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		Assert.Equal(string.Empty, await client.CompleteAsync(Request));
	}

	[Fact]
	public async Task CompleteAsync_ReasoningContentField_IsIgnored()
	{
		var handler = new StubHttpMessageHandler(
			Json(
				"""
				{ "choices": [ { "message": {
				    "reasoning_content": "the user wants to list files",
				    "content": "ls -la" } } ] }
				"""));

		var client = new OpenAICompatibleChatClient(handler.CreateClient(), Options);

		Assert.Equal("ls -la", await client.CompleteAsync(Request));
	}
}
