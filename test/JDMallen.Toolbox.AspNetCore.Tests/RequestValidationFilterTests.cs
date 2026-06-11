using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using FluentValidation;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.AspNetCore.Tests;

public class RequestValidationFilterTests
{
	// Validated by a registered FluentValidation validator.
	private sealed record CreateGadgetRequest(string Name);

	private sealed class GadgetValidator : AbstractValidator<CreateGadgetRequest>
	{
		public GadgetValidator() =>
			RuleFor(request => request.Name).NotEmpty();
	}

	// No validator is registered, so the DataAnnotations fallback applies.
	private sealed record CreateWidgetRequest
	{
		[Required]
		[MinLength(2)]
		public string? Name { get; init; }
	}

	private static void MapGadgets(IEndpointRouteBuilder app) =>
		app.MapPost("/gadgets", (CreateGadgetRequest request) =>
				Results.Ok(request.Name))
			.WithRequestValidation<CreateGadgetRequest>();

	private static void MapWidgets(IEndpointRouteBuilder app) =>
		app.MapPost("/widgets", (CreateWidgetRequest request) =>
				Results.Ok(request.Name))
			.WithRequestValidation<CreateWidgetRequest>();

	[Fact]
	public async Task FluentValidation_RejectsInvalidRequest()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: services =>
				services.AddScoped<IValidator<CreateGadgetRequest>, GadgetValidator>(),
			configureEndpoints: MapGadgets);

		var response = await host.Client.PostAsJsonAsync(
			"/gadgets",
			new CreateGadgetRequest(string.Empty));

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemPayload>();
		Assert.NotNull(problem);
		Assert.Contains(nameof(CreateGadgetRequest.Name), problem!.Errors.Keys);
	}

	[Fact]
	public async Task FluentValidation_AllowsValidRequest()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: services =>
				services.AddScoped<IValidator<CreateGadgetRequest>, GadgetValidator>(),
			configureEndpoints: MapGadgets);

		var response = await host.Client.PostAsJsonAsync(
			"/gadgets",
			new CreateGadgetRequest("sprocket"));

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DataAnnotations_RejectsInvalidRequest()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: MapWidgets);

		var response = await host.Client.PostAsJsonAsync(
			"/widgets",
			new CreateWidgetRequest { Name = "" });

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemPayload>();
		Assert.NotNull(problem);
		Assert.Contains(nameof(CreateWidgetRequest.Name), problem!.Errors.Keys);
	}

	[Fact]
	public async Task DataAnnotations_AllowsValidRequest()
	{
		await using var host = await MinimalApiTestHost.StartAsync(
			configureServices: null,
			configureEndpoints: MapWidgets);

		var response = await host.Client.PostAsJsonAsync(
			"/widgets",
			new CreateWidgetRequest { Name = "ok" });

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private sealed record ValidationProblemPayload
	{
		public Dictionary<string, string[]> Errors { get; init; } = new();
	}
}
