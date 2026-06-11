using JDMallen.Toolbox.AspNetCore.MinimalApi.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for request validation in minimal APIs.
/// </summary>
public static class ValidationExtensions
{
	/// <summary>
	/// Adds request validation using FluentValidation or DataAnnotations.
	/// </summary>
	public static RouteHandlerBuilder WithRequestValidation<TRequest>(
		this RouteHandlerBuilder builder)
		where TRequest : class
		=> builder
			.AddEndpointFilter<RequestValidationFilter<TRequest>>()
			.ProducesValidationProblem();
}
