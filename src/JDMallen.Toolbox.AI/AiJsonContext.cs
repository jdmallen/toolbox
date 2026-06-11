using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
///     Source-generated JSON serialization for all provider wire types, keeping the
///     library trim- and AOT-safe (no reflection-based serialization).
/// </summary>
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(AnthropicRequest))]
[JsonSerializable(typeof(AnthropicResponse))]
[JsonSerializable(typeof(AzureOpenAIRequest))]
[JsonSerializable(typeof(AzureOpenAIResponse))]
internal sealed partial class AiJsonContext : JsonSerializerContext;
