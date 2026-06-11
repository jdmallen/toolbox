using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for IdentityResult in minimal APIs.
/// </summary>
public static class IdentityResultExtensions
{
	extension(IdentityResult result)
	{
		/// <summary>
		/// Executes an action if IdentityResult succeeded, otherwise returns a
		/// validation problem.
		/// </summary>
		public async Task<IResult> Match(Func<Task<IResult>> onSuccess)
			=> result.Succeeded
				? await onSuccess()
				: result.ToValidationProblem();

		/// <summary>
		/// Executes an action if IdentityResult succeeded, otherwise returns a
		/// validation problem.
		/// </summary>
		public IResult Match(Func<IResult> onSuccess)
			=> result.Succeeded
				? onSuccess()
				: result.ToValidationProblem();

		/// <summary>
		/// Converts IdentityResult errors to a dictionary suitable for validation
		/// responses.
		/// </summary>
		public Dictionary<string, string[]> ToErrorDictionary()
		{
			return result.Errors
				.GroupBy(e => e.Code)
				.ToDictionary(
					g => g.Key,
					g => g.Select(e => e.Description).ToArray());
		}

		/// <summary>
		/// Converts IdentityResult errors to a ValidationProblem response.
		/// </summary>
		public ValidationProblem ToValidationProblem()
		{
			Dictionary<string, string[]> errors = result.Errors
				.GroupBy(e => e.Code)
				.ToDictionary(
					g => g.Key,
					g => g.Select(e => e.Description).ToArray());

			return TypedResults.ValidationProblem(errors);
		}
	}
}
