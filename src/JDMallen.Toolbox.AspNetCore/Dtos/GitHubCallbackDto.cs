namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for GitHub OAuth callback parameters.
/// </summary>
public class GitHubCallbackDto
{
	public string Code { get; set; }

	public string State { get; set; }
}
