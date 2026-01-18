using System.ComponentModel.DataAnnotations;

namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for user login requests.
/// </summary>
public class LoginDto
{
	/// <summary>
	/// Email address of the user attempting to log in.
	/// </summary>
	[Required]
	public required string Email { get; set; }

	/// <summary>
	/// Password for the user attempting to log in.
	/// </summary>
	[Required]
	public required string Password { get; set; }
}
