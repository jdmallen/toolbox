using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Results;

/// <summary>
/// Factory for creating standardized Problem Details responses.
/// </summary>
public static class ProblemDetailsResults
{
	/// <summary>
	/// Creates a 400 Bad Request problem details response.
	/// </summary>
	public static ProblemHttpResult BadRequest(
		string detail,
		string? instance = null)
		=> TypedResults.Problem(
			statusCode: 400,
			title: "Bad Request",
			detail: detail,
			instance: instance);

	/// <summary>
	/// Creates a 409 Conflict problem details response.
	/// </summary>
	public static ProblemHttpResult Conflict(
		string detail,
		string? instance = null)
		=> TypedResults.Problem(
			statusCode: 409,
			title: "Conflict",
			detail: detail,
			instance: instance);

	/// <summary>
	/// Creates a 403 Forbidden problem details response.
	/// </summary>
	public static ProblemHttpResult Forbidden(
		string detail,
		string? instance = null)
		=> TypedResults.Problem(
			statusCode: 403,
			title: "Forbidden",
			detail: detail,
			instance: instance);

	/// <summary>
	/// Creates a 500 Internal Server Error problem details response.
	/// </summary>
	public static ProblemHttpResult InternalServerError(
		string detail,
		string? instance = null)
		=> TypedResults.Problem(
			statusCode: 500,
			title: "Internal Server Error",
			detail: detail,
			instance: instance);

	/// <summary>
	/// Creates a 404 Not Found problem details response.
	/// </summary>
	public static ProblemHttpResult NotFound(
		string detail,
		string? instance = null)
		=> TypedResults.Problem(
			statusCode: 404,
			title: "Not Found",
			detail: detail,
			instance: instance);
}
