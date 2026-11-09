namespace JDMallen.Toolbox.AI;

/// <summary>
/// Microsoft Entra ID token scopes accepted by Azure OpenAI inference endpoints.
/// </summary>
/// <remarks>
/// Both scopes were verified against a Foundry (<c>AIServices</c>) resource on
/// both its <c>*.openai.azure.com</c> and <c>*.services.ai.azure.com</c> hosts.
/// The identity also needs a data-plane role on the resource, such as
/// "Cognitive Services OpenAI User"; Owner and Contributor are not enough.
/// </remarks>
public static class AzureOpenAIScopes
{
	/// <summary>The scope current Microsoft Foundry documentation uses.</summary>
	public const string AI_FOUNDRY = "https://ai.azure.com/.default";

	/// <summary>
	/// The scope older Azure OpenAI documentation and samples use. Still
	/// accepted.
	/// </summary>
	public const string COGNITIVE_SERVICES =
		"https://cognitiveservices.azure.com/.default";

	/// <summary>The scope used when none is specified: <see cref="AI_FOUNDRY" />.</summary>
	public const string DEFAULT = AI_FOUNDRY;
}
