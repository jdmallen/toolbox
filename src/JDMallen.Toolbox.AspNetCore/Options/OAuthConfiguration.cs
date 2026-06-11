namespace JDMallen.Toolbox.AspNetCore.Options;

/// <summary>
/// Configuration settings for OAuth authentication providers.
/// </summary>
public class OAuthConfiguration
{
	/// <summary>
	/// Gets or sets the Facebook OAuth client ID.
	/// </summary>
	public string? FacebookClientId { get; set; }

	/// <summary>
	/// Gets or sets the Facebook OAuth client secret.
	/// </summary>
	public string? FacebookClientSecret { get; set; }

	/// <summary>
	/// Gets or sets the GitHub OAuth client ID.
	/// </summary>
	public string? GitHubClientId { get; set; }

	/// <summary>
	/// Gets or sets the GitHub OAuth client secret.
	/// </summary>
	public string? GitHubClientSecret { get; set; }

	/// <summary>
	/// Gets or sets the scopes to request from GitHub OAuth.
	/// </summary>
	public IEnumerable<string> GitHubScopes { get; set; } = [];

	/// <summary>
	/// Gets or sets the Google OAuth client ID.
	/// </summary>
	public string? GoogleClientId { get; set; }

	/// <summary>
	/// Gets or sets the Google OAuth client secret.
	/// </summary>
	public string? GoogleClientSecret { get; set; }

	/// <summary>
	/// Gets or sets the scopes to request from Google OAuth.
	/// </summary>
	public IEnumerable<string> GoogleScopes { get; } = [];

	/// <summary>
	/// Gets or sets the Twitter OAuth client ID.
	/// </summary>
	public string? TwitterClientId { get; set; }

	/// <summary>
	/// Gets or sets the Twitter OAuth client secret.
	/// </summary>
	public string? TwitterClientSecret { get; set; }
}
