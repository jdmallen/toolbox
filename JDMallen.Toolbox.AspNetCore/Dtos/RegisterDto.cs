using System.ComponentModel.DataAnnotations;

namespace JDMallen.Toolbox.AspNetCore.Dtos
{
	public class RegisterDto
	{
		[Required]
		public string Email { get; set; }

		[Required]
		
		public string Password { get; set; }

		[Required]
		public string DisplayName { get; set; }
	}
}
