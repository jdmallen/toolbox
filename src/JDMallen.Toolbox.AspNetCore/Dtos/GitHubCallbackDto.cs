namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for GitHub OAuth callback parameters.
/// </summary>
public record GitHubCallbackDto
{
	/// <summary>
	/// Gets the authorization code returned by GitHub.
	/// </summary>
	public required string Code { get; init; }

	/// <summary>
	/// Gets the state parameter to prevent CSRF attacks.
	/// </summary>
	public required string State { get; init; }
}
