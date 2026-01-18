namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for GitHub OAuth callback parameters.
/// </summary>
public class GitHubCallbackDto
{
	/// <summary>
	/// Gets or sets the authorization code returned by GitHub.
	/// </summary>
	public required string Code { get; set; }

	/// <summary>
	/// Gets or sets the state parameter to prevent CSRF attacks.
	/// </summary>
	public required string State { get; set; }
}
