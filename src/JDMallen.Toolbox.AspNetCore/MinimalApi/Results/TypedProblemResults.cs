using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Results;

/// <summary>
/// Base for the typed problem results (<see cref="NotFoundProblem" />,
/// <see cref="ConflictProblem" />, and so on). Unlike the
/// <see cref="ProblemHttpResult" /> that <see cref="ProblemDetailsResults" />
/// returns, each derived type documents its own status code in OpenAPI, so it
/// can sit in a <c>Results&lt;Ok&lt;T&gt;, NotFoundProblem&gt;</c> union without
/// a separate <c>.ProducesProblem(...)</c> call.
/// </summary>
/// <remarks>
/// Execution is delegated to <see cref="TypedResults.Problem(ProblemDetails)" />,
/// so a registered <c>IProblemDetailsService</c> (for example
/// <c>AddProblemDetails</c> customizations such as a trace ID) still applies.
/// </remarks>
public abstract class TypedProblemResult
	: IResult,
		IStatusCodeHttpResult,
		IContentTypeHttpResult,
		IValueHttpResult,
		IValueHttpResult<ProblemDetails>
{
	private readonly ProblemHttpResult _problem;

	/// <summary>
	/// Initializes the problem with its status code, standard title, and detail.
	/// </summary>
	protected TypedProblemResult(
		int statusCode,
		string title,
		string detail,
		string? instance)
	{
		_problem = TypedResults.Problem(
			statusCode: statusCode,
			title: title,
			detail: detail,
			instance: instance);
	}

	/// <summary>
	/// Gets the problem details written to the response.
	/// </summary>
	public ProblemDetails ProblemDetails => _problem.ProblemDetails;

	/// <inheritdoc />
	public int? StatusCode => _problem.StatusCode;

	/// <inheritdoc />
	public string ContentType => _problem.ContentType;

	/// <inheritdoc />
	object IValueHttpResult.Value => _problem.ProblemDetails;

	/// <inheritdoc />
	ProblemDetails IValueHttpResult<ProblemDetails>.Value => _problem.ProblemDetails;

	/// <inheritdoc />
	public Task ExecuteAsync(HttpContext httpContext) => _problem.ExecuteAsync(httpContext);

	/// <summary>
	/// Adds the OpenAPI metadata for a problem response with the given status.
	/// Derived types call this from their static <c>PopulateMetadata</c>.
	/// </summary>
	protected static void AddProblemMetadata(EndpointBuilder builder, int statusCode)
	{
		ArgumentNullException.ThrowIfNull(builder);

		builder.Metadata.Add(
			new ProducesResponseTypeMetadata(
				statusCode,
				typeof(ProblemDetails),
				["application/problem+json"]));
	}
}

/// <summary>
/// A 400 Bad Request problem result that documents itself in OpenAPI.
/// </summary>
public sealed class BadRequestProblem(string detail, string? instance = null)
	: TypedProblemResult(StatusCodes.Status400BadRequest, "Bad Request", detail, instance),
		IEndpointMetadataProvider
{
	/// <inheritdoc />
	public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
		=> AddProblemMetadata(builder, StatusCodes.Status400BadRequest);
}

/// <summary>
/// A 403 Forbidden problem result that documents itself in OpenAPI.
/// </summary>
public sealed class ForbiddenProblem(string detail, string? instance = null)
	: TypedProblemResult(StatusCodes.Status403Forbidden, "Forbidden", detail, instance),
		IEndpointMetadataProvider
{
	/// <inheritdoc />
	public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
		=> AddProblemMetadata(builder, StatusCodes.Status403Forbidden);
}

/// <summary>
/// A 404 Not Found problem result that documents itself in OpenAPI.
/// </summary>
public sealed class NotFoundProblem(string detail, string? instance = null)
	: TypedProblemResult(StatusCodes.Status404NotFound, "Not Found", detail, instance),
		IEndpointMetadataProvider
{
	/// <inheritdoc />
	public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
		=> AddProblemMetadata(builder, StatusCodes.Status404NotFound);
}

/// <summary>
/// A 409 Conflict problem result that documents itself in OpenAPI.
/// </summary>
public sealed class ConflictProblem(string detail, string? instance = null)
	: TypedProblemResult(StatusCodes.Status409Conflict, "Conflict", detail, instance),
		IEndpointMetadataProvider
{
	/// <inheritdoc />
	public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
		=> AddProblemMetadata(builder, StatusCodes.Status409Conflict);
}

/// <summary>
/// A 500 Internal Server Error problem result that documents itself in OpenAPI.
/// </summary>
public sealed class InternalServerErrorProblem(string detail, string? instance = null)
	: TypedProblemResult(
			StatusCodes.Status500InternalServerError,
			"Internal Server Error",
			detail,
			instance),
		IEndpointMetadataProvider
{
	/// <inheritdoc />
	public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
		=> AddProblemMetadata(builder, StatusCodes.Status500InternalServerError);
}
