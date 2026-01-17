using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for IdentityResult in minimal APIs.
/// </summary>
public static class IdentityResultExtensions
{
	/// <summary>
	/// Converts IdentityResult errors to a ValidationProblem response.
	/// </summary>
	public static ValidationProblem ToValidationProblem(
		this IdentityResult result)
	{
		var errors = result.Errors
			.GroupBy(e => e.Code)
			.ToDictionary(
				g => g.Key,
				g => g.Select(e => e.Description).ToArray());

		return TypedResults.ValidationProblem(errors);
	}

	/// <summary>
	/// Converts IdentityResult errors to a dictionary suitable for validation
	/// responses.
	/// </summary>
	public static Dictionary<string, string[]> ToErrorDictionary(
		this IdentityResult result)
	{
		return result.Errors
			.GroupBy(e => e.Code)
			.ToDictionary(
				g => g.Key,
				g => g.Select(e => e.Description).ToArray());
	}

	/// <summary>
	/// Executes an action if IdentityResult succeeded, otherwise returns a
	/// validation problem.
	/// </summary>
	public static async Task<IResult> Match(
		this IdentityResult result,
		Func<Task<IResult>> onSuccess)
	{
		return result.Succeeded
			? await onSuccess()
			: result.ToValidationProblem();
	}

	/// <summary>
	/// Executes an action if IdentityResult succeeded, otherwise returns a
	/// validation problem.
	/// </summary>
	public static IResult Match(
		this IdentityResult result,
		Func<IResult> onSuccess)
	{
		return result.Succeeded
			? onSuccess()
			: result.ToValidationProblem();
	}
}
