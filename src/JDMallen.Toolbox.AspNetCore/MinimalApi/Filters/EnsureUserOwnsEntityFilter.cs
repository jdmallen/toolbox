using System.Reflection;
using JDMallen.Toolbox.AspNetCore.Extensions;
using JDMallen.Toolbox.AspNetCore.MinimalApi.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
		var dbContext = context.HttpContext.RequestServices
			.GetRequiredService<DbContext>();

		var userId = context.HttpContext.User.GetUserIdAsGuid();

		// Extract entity ID from route or request
		var entityId = ExtractEntityId(context);
		if (!entityId.HasValue) return await next(context);

		var entity = await dbContext.Set<TEntity>()
			.AsNoTracking()
			.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId.Value);

		return entity switch
		{
			null => ProblemDetailsResults.NotFound(
				$"{typeof(TEntity).Name} not found."),
			_ when entity.UserId != userId => ProblemDetailsResults.Forbidden(
				"You do not have permission to access this resource."),
			_ => await next(context)
		};
	}

	private static Guid? ExtractEntityId(EndpointFilterInvocationContext context)
	{
		// Try route values first
		if (context.HttpContext.Request.RouteValues.TryGetValue(
			    "id",
			    out var routeId))
			if (Guid.TryParse(routeId?.ToString(), out var guid))
				return guid;

		// Try request arguments with Id property
		foreach (var arg in context.Arguments)
		{
			if (arg is null) continue;

			var idProperty = arg.GetType()
				.GetProperty("Id", BindingFlags.IgnoreCase | BindingFlags.Public);
			if (idProperty?.PropertyType == typeof(Guid))
				return (Guid?)idProperty.GetValue(arg);
		}

		return null;
	}
}
