using System.ComponentModel.DataAnnotations;

namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for user registration requests.
/// </summary>
public class RegisterDto
{
	/// <summary>
	/// Email address of the user to register.
	/// </summary>
	[Required]
	public required string Email { get; set; }

	/// <summary>
	/// Password for the new user account.
	/// </summary>
	[Required]
	public required string Password { get; set; }

	/// <summary>
	/// Display name of the user.
	/// </summary>
	[Required]
	public required string DisplayName { get; set; }
}
