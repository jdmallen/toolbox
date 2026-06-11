using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="IdentityResultExtensions" /> bridges
/// <see cref="IdentityResult" /> outcomes onto minimal API problem responses.
/// </summary>
public class IdentityResultExtensionsTests
{
	private static IdentityResult FailedResult() =>
		IdentityResult.Failed(
			new IdentityError
			{
				Code = "PasswordTooShort",
				Description = "Too short.",
			},
			new IdentityError
			{
				Code = "PasswordTooShort",
				Description = "Need 8+ chars.",
			},
			new IdentityError
			{
				Code = "DuplicateEmail",
				Description = "Email in use.",
			});

	[Fact]
	public void Match_ReturnsSuccessResultWhenSucceeded()
	{
		IResult expected = Results.Ok("created");

		IResult actual = IdentityResult.Success.Match(() => expected);

		Assert.Same(expected, actual);
	}

	[Fact]
	public void Match_ReturnsValidationProblemWhenFailed()
	{
		IResult actual = FailedResult().Match(() => Results.Ok("created"));

		Assert.IsType<ValidationProblem>(actual);
	}

	[Fact]
	public async Task MatchAsync_ReturnsSuccessResultWhenSucceeded()
	{
		IResult expected = Results.Ok("created");

		IResult actual = await IdentityResult.Success.Match(() => Task.FromResult(expected));

		Assert.Same(expected, actual);
	}

	[Fact]
	public async Task MatchAsync_ReturnsValidationProblemWhenFailed()
	{
		IResult actual = await FailedResult().Match(() => Task.FromResult(Results.Ok("created")));

		Assert.IsType<ValidationProblem>(actual);
	}

	[Fact]
	public void ToErrorDictionary_GroupsDescriptionsByErrorCode()
	{
		Dictionary<string, string[]> errors = FailedResult().ToErrorDictionary();

		Assert.Equal(2, errors.Count);
		Assert.Equal(
			["Too short.", "Need 8+ chars."],
			errors["PasswordTooShort"]);
		Assert.Equal(["Email in use."], errors["DuplicateEmail"]);
	}

	[Fact]
	public void ToValidationProblem_ReturnsStatus400WithGroupedErrors()
	{
		var problem = FailedResult().ToValidationProblem();

		Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
		Assert.Contains("PasswordTooShort", problem.ProblemDetails.Errors.Keys);
		Assert.Contains("DuplicateEmail", problem.ProblemDetails.Errors.Keys);
	}
}
