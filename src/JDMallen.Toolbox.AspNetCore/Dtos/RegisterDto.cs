using System.ComponentModel.DataAnnotations;

namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for user registration requests.
/// </summary>
public record RegisterDto
{
	/// <summary>
	/// Display name of the user.
	/// </summary>
	[Required]
	public required string DisplayName { get; init; }

	/// <summary>
	/// Email address of the user to register.
	/// </summary>
	[Required]
	public required string Email { get; init; }

	/// <summary>
	/// Password for the new user account.
	/// </summary>
	[Required]
	public required string Password { get; init; }
}
