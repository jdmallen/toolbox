using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JDMallen.Toolbox.AspNetCore.Extensions;

/// <summary>
/// Extension methods for ASP.NET Core Identity operations.
/// </summary>
public static class IdentityExtensions
{
	/// <param name="modelState">The ModelState dictionary.</param>
	extension(ModelStateDictionary modelState)
	{
		/// <summary>
		/// Adds a single Identity error to the ModelState.
		/// </summary>
		/// <param name="code">The error code.</param>
		/// <param name="description">The error description.</param>
		/// <returns>The updated ModelState dictionary.</returns>
		public ModelStateDictionary AddIdentityError(
			string code,
			string description)
		{
			modelState.TryAddModelError(code, description);

			return modelState;
		}

		/// <summary>
		/// Adds all Identity errors from an IdentityResult to the ModelState.
		/// </summary>
		/// <param name="result">The Identity result containing errors.</param>
		/// <returns>The updated ModelState dictionary.</returns>
		public ModelStateDictionary AddIdentityErrors(IdentityResult result)
		{
			result.Errors
				.ToList()
				.ForEach(error =>
					modelState.TryAddModelError(error.Code, error.Description));

			return modelState;
		}
	}
}
