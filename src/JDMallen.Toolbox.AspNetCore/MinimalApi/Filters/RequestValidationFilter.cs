using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Filters;

/// <summary>
/// Endpoint filter that validates requests using FluentValidation or
/// DataAnnotations.
/// </summary>
public class RequestValidationFilter<TRequest> : IEndpointFilter
	where TRequest : class
{
	/// <summary>
	/// Invokes the filter to validate the request before processing.
	/// </summary>
	/// <param name="context">The endpoint filter invocation context.</param>
	/// <param name="next">The next filter in the pipeline.</param>
	/// <returns>A validation problem result if validation fails, otherwise the result of the next filter.</returns>
	public async ValueTask<object> InvokeAsync(
		EndpointFilterInvocationContext context,
		EndpointFilterDelegate next)
	{
		var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
		if (request is null) return await next(context);

		// Try FluentValidation first
		var validator = context.HttpContext.RequestServices
			.GetService(typeof(IValidator<TRequest>)) as IValidator<TRequest>;

		if (validator is not null)
		{
			var validationResult = await validator.ValidateAsync(
				request,
				context.HttpContext.RequestAborted);

			if (!validationResult.IsValid)
				return TypedResults.ValidationProblem(validationResult.ToDictionary());
		}
		else
		{
			// Fallback to DataAnnotations
			var validationResults = new List<ValidationResult>();
			var validationContext = new ValidationContext(request);

			if (!Validator.TryValidateObject(
				    request,
				    validationContext,
				    validationResults,
				    true))
			{
				var errors = validationResults
					.GroupBy(x => x.MemberNames.FirstOrDefault() ?? string.Empty)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage ?? string.Empty).ToArray());

				return TypedResults.ValidationProblem(errors);
			}
		}

		return await next(context);
	}
}
