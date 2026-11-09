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
	Task<CompletionResult> GetCompletionAsync(
		CompletionRequest request,
		CancellationToken cancellationToken = default);
}
```

It returns the completion text (empty string if the model returned none) and
throws `HttpRequestException` on a non-success status, with the response body in
the message. Prompt building and response parsing stay in your code, so
switching providers is a registration change.

`CompletionResult` carries what the provider reported alongside the text. Every
member except `Text` is null when the response left it out (self-hosted
runners in particular may not report usage):

| Member       | Anthropic                                           | Azure / OpenAI-compatible     |
|--------------|-----------------------------------------------------|-------------------------------|
| `Text`       | concatenated `text` content blocks                  | `choices[0].message.content`  |
| `Usage`      | `TokenUsage(InputTokens, OutputTokens)`, see below  | `prompt_tokens`, `completion_tokens` |
| `Model`      | `model`                                             | `model`                       |
| `Id`         | `id`                                                | `id`                          |
| `StopReason` | `stop_reason` (`end_turn`, `max_tokens`, ...)       | `choices[0].finish_reason` (`stop`, `length`, ...) |

`TokenUsage.InputTokens` counts every billed prompt token on both providers:
for Anthropic it adds the cache-creation and cache-read counters to
`input_tokens`, matching OpenAI's `prompt_tokens`, which already includes
cached tokens. `OutputTokens` includes hidden reasoning tokens.
`TotalTokens` is their sum.

```csharp
CompletionResult result = await client.GetCompletionAsync(request, ct);
Console.WriteLine(result.Text);
if (result.Usage is { } usage)
{
	Console.WriteLine($"{usage.TotalTokens} tokens");
}
```

`CompleteAsync`, which returns only the text, is deprecated since 4.3.0 and
will be removed in 5.0.0. Its replacement has a default implementation that
calls it, so a custom `IChatCompletionClient` written for 4.2.x keeps
compiling (reporting no metadata) until it implements `GetCompletionAsync`.

```csharp
var request = new CompletionRequest(
	SystemPrompt: "You are a terse assistant.",
	UserPrompt: "Name three primary colors.",
	MaxTokens: 200,
	Temperature: 0.1);  // optional, defaults to 0.1
```

## Providers

| Client                       | Options                                                  | Auth header                                      |
|------------------------------|----------------------------------------------------------|--------------------------------------------------|
| `AnthropicChatClient`        | `AnthropicClientOptions(ApiKey, Model)`                  | `x-api-key`                                      |
| `AzureOpenAIChatClient`      | `AzureOpenAIClientOptions(ApiKey, Endpoint, Deployment)` | `api-key`, or `Authorization: Bearer` with Entra ID |
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
`MaxTokens` is sent as `max_completion_tokens`, which every Azure chat model
accepts; reasoning models reject the deprecated `max_tokens`. `top_p` and the
penalties are left at the service defaults.

For a reasoning deployment, set `IsReasoningModel`. Reasoning models reject
sampling parameters, so `temperature` is then left out. Their hidden reasoning
is also spent from `MaxTokens`, so give them a larger budget (a few thousand
tokens) or the answer can come back empty:

```csharp
var client = new AzureOpenAIChatClient(
	httpClient,
	new AzureOpenAIClientOptions(apiKey, endpoint, "o4-mini")
	{
		IsReasoningModel = true,
	});
```

The endpoint is the resource host, e.g. `https://myresource.openai.azure.com`
or `https://myresource.services.ai.azure.com`. A trailing `/openai` or
`/openai/v1` copied from the portal is removed; any other path (such as an API
Management prefix) is kept.

### Azure OpenAI with Microsoft Entra ID (no API key)

Set a token provider instead of a key. Requests then carry
`Authorization: Bearer <token>` and no `api-key` header:

```csharp
var tokens = new CachingBearerTokenProvider(
	new AzureCliTokenProvider(new AzureCliTokenProviderOptions
	{
		TenantId = "contoso.onmicrosoft.com", // optional; default: az's current tenant
	}));

var client = new AzureOpenAIChatClient(
	httpClient,
	AzureOpenAIClientOptions.ForEntraId(
		"https://myresource.openai.azure.com",
		"gpt-6-luna",
		tokens));
```

- `AzureCliTokenProvider` runs
  `az account get-access-token --scope <scope> [--tenant <tenant>] --output json`,
  so whoever ran `az login` (with their MFA and Conditional Access) is the
  identity. Needs Azure CLI 2.54 or later. No Azure SDK dependency, so it stays
  trim- and AOT-safe. Set `AzExecutablePath` when `az` is not on the app's
  PATH.
- `CachingBearerTokenProvider` reuses the token until 5 minutes before it
  expires and shares one fetch between concurrent callers. Without it, every
  request starts an `az` process.
- The scope defaults to `AzureOpenAIScopes.Default` (`https://ai.azure.com/.default`).
  `AzureOpenAIScopes.CognitiveServices` is also accepted by Azure OpenAI and
  Foundry resources.
- `IBearerTokenProvider` is the seam: implement it to use any other source of
  tokens, such as an Azure.Identity `TokenCredential`.

Setup on the Azure side:

1. The identity needs a data-plane role on the resource, such as
   "Cognitive Services OpenAI User". Owner and Contributor are not enough. New
   assignments take up to 5 minutes to apply.
2. The endpoint must be the resource's own custom subdomain (not a regional
   endpoint).
3. Sign in: `az login --scope https://ai.azure.com/.default`.

A 401 or 403 adds a hint covering these to the `HttpRequestException` message,
and the exception's `StatusCode` is set. Failures getting a token throw
`AzureCliCredentialException`, whose message says what to run. Tokens are only
sent over https (or http to loopback), and never appear in exception messages.

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

## Token Usage Totals

`CompletionResult.Usage` covers one request. To keep a running count, add each
request's usage to a `TokenUsageTally`, then fold the run into a lifetime total
on disk when it ends:

```csharp
var run = new TokenUsageTally();
run.Add(result.Usage); // null (no usage reported) is ignored

string path = TokenUsageLog.DefaultPath("myapp"); // ~/.local/share/myapp/usage.json
if (!TokenUsageLog.TryRecord(path, run, DateTimeOffset.Now, out string? error))
{
	Console.Error.WriteLine($"Usage not recorded: {error}");
}

TokenUsageTotals? totals = TokenUsageLog.Read(path, out error);
TokenUsageAverages? perRun = totals?.AveragePerRun();
```

- A run with no reported usage isn't counted, so it doesn't pull the average
  down.
- Several processes can record at once: updates take a lock file next to
  `usage.json` and are written through a temporary file.
- A file that won't parse is reported through `error` and left as it is, never
  overwritten.

## Testing

Depend on `IChatCompletionClient` in your code and substitute it in tests. To
test a client itself, pass an `HttpClient` built on a stub `HttpMessageHandler`.

## Target Framework

- `net10.0` (marked `IsTrimmable` and `IsAotCompatible`)

## Dependencies

None beyond the .NET base class library.

## License

MIT. See the repository root for the full license text.
