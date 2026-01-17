using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Filters;

/// <summary>
/// Endpoint filter that validates requests using FluentValidation or DataAnnotations.
/// </summary>
public class RequestValidationFilter<TRequest> : IEndpointFilter
	where TRequest : class
{
	public async ValueTask<object> InvokeAsync(
		EndpointFilterInvocationContext context,
		EndpointFilterDelegate next)
	{
		throw new System.NotImplementedException();
	}
}
