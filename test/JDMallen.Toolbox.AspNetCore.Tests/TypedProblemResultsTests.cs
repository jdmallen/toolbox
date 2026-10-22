using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Results;
using JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies the typed problem results: each carries the same status, title, and
/// detail as its <see cref="ProblemDetailsResults" /> counterpart, documents its
/// own status in endpoint metadata, and writes an RFC 7807 response.
/// </summary>
public class TypedProblemResultsTests
{
	private static readonly MethodInfo AnyMethod =
		typeof(TypedProblemResultsTests).GetMethod(nameof(AnyMethodTarget), BindingFlags.NonPublic | BindingFlags.Static)!;

	private static void AnyMethodTarget()
	{
	}

	// Rows carry the result's Type (serializable, so Test Explorer lists each
	// row) and the test constructs it.
	public static TheoryData<Type, int, string> Problems => new()
	{
		{ typeof(BadRequestProblem), StatusCodes.Status400BadRequest, "Bad Request" },
		{ typeof(ForbiddenProblem), StatusCodes.Status403Forbidden, "Forbidden" },
		{ typeof(NotFoundProblem), StatusCodes.Status404NotFound, "Not Found" },
		{ typeof(ConflictProblem), StatusCodes.Status409Conflict, "Conflict" },
		{
			typeof(InternalServerErrorProblem),
			StatusCodes.Status500InternalServerError,
			"Internal Server Error"
		},
	};

	[Theory]
	[MemberData(nameof(Problems))]
	public void Problem_HasStatusTitleAndDetail(Type problemType, int status, string title)
	{
		var problem = (TypedProblemResult)Activator.CreateInstance(problemType, "detail", null)!;

		Assert.Equal(status, problem.StatusCode);
		Assert.Equal(status, problem.ProblemDetails.Status);
		Assert.Equal(title, problem.ProblemDetails.Title);
		Assert.Equal("detail", problem.ProblemDetails.Detail);
		Assert.Equal("application/problem+json", problem.ContentType);
	}

	[Fact]
	public void Instance_IsPropagatedWhenProvided()
	{
		var problem = new NotFoundProblem("Gadget 7 was not found.", "/gadgets/7");

		Assert.Equal("/gadgets/7", problem.ProblemDetails.Instance);
	}

	[Fact]
	public void PopulateMetadata_DocumentsOwnStatusAsProblemDetails()
	{
		var builder = new RouteEndpointBuilder(
			_ => Task.CompletedTask,
			RoutePatternFactory.Parse("/"),
			0);

		NotFoundProblem.PopulateMetadata(AnyMethod, builder);
		ConflictProblem.PopulateMetadata(AnyMethod, builder);

		int[] documentedStatuses = builder.Metadata
			.OfType<IProducesResponseTypeMetadata>()
			.Where(metadata => metadata.Type == typeof(ProblemDetails))
			.Select(metadata => metadata.StatusCode)
			.Order()
			// ReSharper disable once UseCollectionExpression
			.ToArray();

		Assert.Equal([StatusCodes.Status404NotFound, StatusCodes.Status409Conflict], documentedStatuses);
	}

	[Fact]
	public async Task Endpoint_ReturningNotFoundProblem_WritesProblemJson()
	{
		await using MinimalApiTestHost host = await MinimalApiTestHost.StartAsync(
			null,
			app => app.MapGet(
				"/gadgets/{id:int}",
				Results<Ok<string>, NotFoundProblem> (int id)
					=> new NotFoundProblem($"Gadget {id} was not found.")));

		HttpResponseMessage response = await host.Client.GetAsync("/gadgets/7");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

		var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		Assert.Equal("Not Found", body?.Title);
		Assert.Equal("Gadget 7 was not found.", body?.Detail);
	}
}
