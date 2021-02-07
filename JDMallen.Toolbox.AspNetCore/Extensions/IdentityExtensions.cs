using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JDMallen.Toolbox.AspNetCore.Extensions
{
	public static class IdentityExtensions
	{
		public static ModelStateDictionary AddIdentityErrors(
			this ModelStateDictionary modelState,
			IdentityResult result)
		{
			result.Errors
				.ToList()
				.ForEach(error => modelState.TryAddModelError(error.Code, error.Description));
			return modelState;
		}

		public static ModelStateDictionary AddIdentityError(
			this ModelStateDictionary modelState,
			string code,
			string description)
		{
			modelState.TryAddModelError(code, description);
			return modelState;
		}
	}
}
