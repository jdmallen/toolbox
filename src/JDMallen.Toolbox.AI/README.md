# JDMallen.Toolbox.AI

Minimal chat completion clients for Anthropic, Azure OpenAI, and any
OpenAI-compatible server (openai.com, llama.cpp, llama-swap, Ollama, vLLM,
LM Studio). Raw `HttpClient` plus source-generated JSON: no vendor SDKs,
trim-safe, and AOT-compatible.

## Installation

```bash
dotnet add package JDMallen.Toolbox.AI
```

## The Interface

Every provider implements one method:

```csharp
public interface IChatCompletionClient
{
	Task<string> CompleteAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default);
}
```

It returns the completion text (empty string if the model returned none) and
throws `HttpRequestException` on a non-success status, with the response body in
the message. Prompt building and response parsing stay in your code, so
switching providers is a registration change.

```csharp
var request = new CompletionRequest(
	SystemPrompt: "You are a terse assistant.",
	UserPrompt: "Name three primary colors.",
	MaxTokens: 200,
	Temperature: 0.1);  // optional, defaults to 0.1
```

## Providers

| Client                       | Options                                          | Auth header             |
|------------------------------|--------------------------------------------------|-------------------------|
| `AnthropicChatClient`        | `AnthropicClientOptions(ApiKey, Model)`          | `x-api-key`             |
| `AzureOpenAIChatClient`      | `AzureOpenAIClientOptions(ApiKey, Endpoint, Deployment)` | `api-key`       |
| `OpenAICompatibleChatClient` | `OpenAICompatibleClientOptions(BaseUrl, Model, ApiKey?)` | `Authorization: Bearer`, only if `ApiKey` is set |

Every client takes an `HttpClient` you own. Requests use absolute URLs, so no
`BaseAddress` is needed.

### Anthropic

```csharp
builder.Services.AddHttpClient<IChatCompletionClient, AnthropicChatClient>();
builder.Services.AddSingleton(new AnthropicClientOptions(
	builder.Configuration["Anthropic:ApiKey"]!,
	"claude-haiku-4-5-20251001"));
```

Posts to the Messages API (`anthropic-version: 2023-06-01`) and concatenates
the `text` content blocks of the response.

### Azure OpenAI

```csharp
var client = new AzureOpenAIChatClient(
	httpClient,
	new AzureOpenAIClientOptions(
		apiKey,
		"https://myresource.openai.azure.com",
		"gpt-4o-mini"));
```

The deployment goes in the URL path, and `api-version=2024-10-21` is sent.

### OpenAI-compatible and self-hosted

```csharp
var client = new OpenAICompatibleChatClient(
	new HttpClient { Timeout = TimeSpan.FromMinutes(10) },
	new OpenAICompatibleClientOptions(
		BaseUrl: "http://localhost:8080/v1",
		Model: "qwen3-8b"));
```

- `BaseUrl` is the API root up to the version segment; `/chat/completions` is
  appended. A trailing slash is fine.
- With no `ApiKey` (null, empty, or whitespace), no `Authorization` header is
  sent, which is what most local runners expect.
- A cold local model can take minutes to load, so raise the `HttpClient`
  timeout above the 100-second default.

**Reasoning models.** This client drops any `reasoning_content` field and strips
inline `<think>`, `<thinking>`, and `<reasoning>` blocks from the answer. If the
response *starts* with an unclosed tag, generation was cut off mid-thought
(usually by `MaxTokens`), and you get an empty string rather than the partial
reasoning. An unclosed tag later in the text is kept as part of the answer. The
Anthropic and Azure clients do not do this filtering.

## Images

Attach images for vision-capable models with the `Images` init property:

```csharp
byte[] photo = await File.ReadAllBytesAsync("receipt.jpg", cancellationToken);

var request = new CompletionRequest(
	"Extract the total from this receipt.",
	"What is the total?",
	300)
{
	Images = [new ImageInput(photo, "image/jpeg")],
};
```

- `ImageInput.Data` is the raw image bytes, not base64; the client encodes them.
- Images are sent before the prompt text: as base64 `image` blocks for
  Anthropic, and as `image_url` data URIs for Azure and OpenAI-compatible
  servers.
- With no images, the user message is a plain string, exactly as before image
  support existed. Some self-hosted runners reject the array form, so text-only
  requests never use it.
- Self-hosted servers need a vision model; for llama.cpp, one loaded with its
  `mmproj` projector. Supported media types vary by provider (Anthropic accepts
  jpeg, png, gif, and webp).

## Testing

Depend on `IChatCompletionClient` in your code and substitute it in tests. To
test a client itself, pass an `HttpClient` built on a stub `HttpMessageHandler`.

## Target Framework

- `net10.0` (marked `IsTrimmable` and `IsAotCompatible`)

## Dependencies

None beyond the .NET base class library.

## License

MIT. See the repository root for the full license text.
