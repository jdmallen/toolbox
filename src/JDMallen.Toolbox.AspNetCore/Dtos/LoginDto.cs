using System.ComponentModel.DataAnnotations;

namespace JDMallen.Toolbox.AspNetCore.Dtos;

/// <summary>
/// Data transfer object for user login requests.
/// </summary>
public class LoginDto
{
	[Required] public string Email { get; set; }

	[Required] public string Password { get; set; }
}
