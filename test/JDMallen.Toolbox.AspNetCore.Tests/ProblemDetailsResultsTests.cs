using JDMallen.Toolbox.AspNetCore.MinimalApi.Results;
using Microsoft.AspNetCore.Http;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="ProblemDetailsResults"/> produces RFC 7807 problem
/// responses with the expected status code, title, and detail for each helper.
/// </summary>
public class ProblemDetailsResultsTests
{
	[Fact]
	public void NotFound_ProducesStatus404WithTitleAndDetail()
	{
		var result = ProblemDetailsResults.NotFound("Gadget 7 was not found.");

		Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
		Assert.Equal(StatusCodes.Status404NotFound, result.ProblemDetails.Status);
		Assert.Equal("Not Found", result.ProblemDetails.Title);
		Assert.Equal("Gadget 7 was not found.", result.ProblemDetails.Detail);
	}

	[Fact]
	public void BadRequest_ProducesStatus400()
	{
		var result = ProblemDetailsResults.BadRequest("The payload was malformed.");

		Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
		Assert.Equal("Bad Request", result.ProblemDetails.Title);
		Assert.Equal("The payload was malformed.", result.ProblemDetails.Detail);
	}

	[Fact]
	public void Conflict_ProducesStatus409()
	{
		var result = ProblemDetailsResults.Conflict("That name is already taken.");

		Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
		Assert.Equal("Conflict", result.ProblemDetails.Title);
	}

	[Fact]
	public void Forbidden_ProducesStatus403()
	{
		var result = ProblemDetailsResults.Forbidden("You may not touch this.");

		Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
		Assert.Equal("Forbidden", result.ProblemDetails.Title);
	}

	[Fact]
	public void InternalServerError_ProducesStatus500()
	{
		var result = ProblemDetailsResults.InternalServerError("Something broke.");

		Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
		Assert.Equal("Internal Server Error", result.ProblemDetails.Title);
	}

	[Fact]
	public void Instance_IsPropagatedWhenProvided()
	{
		var result = ProblemDetailsResults.NotFound(
			"Gadget 7 was not found.",
			instance: "/gadgets/7");

		Assert.Equal("/gadgets/7", result.ProblemDetails.Instance);
	}
}
