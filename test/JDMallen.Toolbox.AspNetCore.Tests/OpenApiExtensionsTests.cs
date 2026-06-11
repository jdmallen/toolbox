using System.Net;
using System.Text.Json;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;
using JDMallen.Toolbox.AspNetCore.Tests.Infrastructure;
using Microsoft.AspNetCore.Builder;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies that <see cref="OpenApiExtensions.AddOpenApiWithJwt" /> emits a
/// document carrying the supplied title and a Bearer security scheme. The latter
/// guards the fix for the null-conditional indexer that previously dropped the
/// scheme silently.
/// </summary>
public class OpenApiExtensionsTests
{
	private static Task<MinimalApiTestHost> StartDocumentHostAsync() =>
		MinimalApiTestHost.StartAsync(
			services =>
				services.AddOpenApiWithJwt("Gadget API"),
			app => app.MapOpenApi());

	private static async Task<JsonElement> GetDocumentRootAsync(MinimalApiTestHost host)
	{
		HttpResponseMessage response = await host.Client.GetAsync("/openapi/v1.json");
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		await using Stream stream = await response.Content.ReadAsStreamAsync();
		using JsonDocument document = await JsonDocument.ParseAsync(stream);

		return document.RootElement.Clone();
	}

	[Fact]
	public async Task AddOpenApiWithJwt_RegistersBearerSecurityScheme()
	{
		await using MinimalApiTestHost host = await StartDocumentHostAsync();

		JsonElement root = await GetDocumentRootAsync(host);

		JsonElement bearer = root
			.GetProperty("components")
			.GetProperty("securitySchemes")
			.GetProperty("Bearer");

		Assert.Equal("http", bearer.GetProperty("type").GetString());
		Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
		Assert.Equal("JWT", bearer.GetProperty("bearerFormat").GetString());
	}

	[Fact]
	public async Task AddOpenApiWithJwt_SetsDocumentTitle()
	{
		await using MinimalApiTestHost host = await StartDocumentHostAsync();

		JsonElement root = await GetDocumentRootAsync(host);

		Assert.Equal(
			"Gadget API",
			root.GetProperty("info").GetProperty("title").GetString());
	}
}
