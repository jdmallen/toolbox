namespace JDMallen.Toolbox.AI.Azure;

/// <summary>Which Azure.Identity credential to sign in with.</summary>
public enum AzureCredentialKind
{
	/// <summary>
	/// <c>DefaultAzureCredential</c>: environment, workload and managed
	/// identity, then developer tools. Set the <c>AZURE_TOKEN_CREDENTIALS</c>
	/// environment variable to <c>dev</c>, <c>prod</c>, or one credential name
	/// (e.g. <c>AzureCliCredential</c>) to narrow the chain.
	/// </summary>
	Default,

	/// <summary>The signed-in Azure CLI (<c>az</c>).</summary>
	AzureCli,

	/// <summary>The signed-in Azure Developer CLI (<c>azd</c>).</summary>
	AzureDeveloperCli,

	/// <summary>
	/// A browser sign-in, remembered in a persistent token cache so later runs
	/// are silent.
	/// </summary>
	InteractiveBrowser,

	/// <summary>
	/// A code to enter at a sign-in page on any device, for machines without a
	/// browser; remembered like <see cref="InteractiveBrowser" />.
	/// </summary>
	DeviceCode,
}

/// <summary>Settings for <see cref="AzureCredentialFactory" />.</summary>
public sealed record AzureCredentialOptions
{
	/// <summary>The token cache name used when none is set.</summary>
	public const string DEFAULT_CACHE_NAME = "jdmallen-toolbox-ai";

	/// <summary>
	/// The credential to use; <see cref="AzureCredentialKind.Default" /> unless
	/// set.
	/// </summary>
	public AzureCredentialKind Kind { get; init; } = AzureCredentialKind.Default;

	/// <summary>
	/// The Entra tenant (GUID or domain) to sign in to, or null for the
	/// credential's own default.
	/// </summary>
	public string? TenantId { get; init; }

	/// <summary>
	/// The persistent token cache's name, so apps don't share (or clobber) one
	/// another's sign-ins. Interactive kinds only.
	/// </summary>
	public string CacheName { get; init; } = DEFAULT_CACHE_NAME;

	/// <summary>
	/// Where to keep the sign-in's <c>AuthenticationRecord</c> (account and
	/// tenant ids; no tokens), e.g. <c>~/.config/committy/auth-record.json</c>.
	/// With it, a new process finds its cached token silently instead of
	/// prompting again. Interactive kinds only; null keeps nothing.
	/// </summary>
	public string? AuthenticationRecordPath { get; init; }

	/// <summary>
	/// Whether an interactive kind may prompt (open a browser, or print a
	/// device code) when it has no usable cached sign-in. Set false where a
	/// prompt would be wrong, e.g. inside a git hook: the token request then
	/// fails with <c>AuthenticationRequiredException</c> instead.
	/// </summary>
	public bool AllowInteractiveAuthentication { get; init; } = true;
}

/// <summary>Who <see cref="AzureCredentialFactory.SignInAsync" /> signed in.</summary>
/// <param name="Username">
/// The account, for the interactive kinds; null for the others, which report
/// no account.
/// </param>
/// <param name="TenantId">
/// The account's tenant, for the interactive kinds; null for the others.
/// </param>
public sealed record AzureSignInResult(string? Username, string? TenantId);
