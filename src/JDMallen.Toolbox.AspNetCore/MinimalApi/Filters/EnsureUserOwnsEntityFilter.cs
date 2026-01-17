using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Filters;

/// <summary>
/// Marker interface for entities with ownership.
/// </summary>
public interface IOwnedEntity
{
	Guid UserId { get; }
}

/// <summary>
/// Endpoint filter that ensures the current user owns the requested entity.
/// </summary>
public class EnsureUserOwnsEntityFilter<TEntity> : IEndpointFilter
	where TEntity : class, IOwnedEntity
{
	public async ValueTask<object> InvokeAsync(
		EndpointFilterInvocationContext context,
		EndpointFilterDelegate next)
	{
		throw new NotImplementedException();
	}
}
