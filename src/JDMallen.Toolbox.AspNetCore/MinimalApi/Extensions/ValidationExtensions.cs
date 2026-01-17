using Microsoft.AspNetCore.Builder;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Extensions;

/// <summary>
/// Extension methods for request validation in minimal APIs.
/// </summary>
public static class ValidationExtensions
{
	/// <summary>
	/// Adds request validation using FluentValidation or DataAnnotations.
	/// </summary>
	public static RouteHandlerBuilder WithRequestValidation<TRequest>(this RouteHandlerBuilder builder)
		where TRequest : class
	{
		throw new System.NotImplementedException();
	}
}
