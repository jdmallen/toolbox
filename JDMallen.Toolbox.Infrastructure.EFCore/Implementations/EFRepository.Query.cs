using System.Linq;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Filters;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepository<TDomainModel, TEntityModel, TQueryParameters, TId>
		where TDomainModel : class, IDomainModel<TId>
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		protected IQueryable<TEntityModel> BuildQueryInit(
			TQueryParameters parameters,
			IQueryable<TEntityModel> query = null)
		{
			if (query == null)
				query = Context.GetQueryable<TEntityModel>();

			if (parameters == null)
				return query;

			if (!parameters.TrackEntities)
				query = query.AsNoTracking();

			if (parameters.AutomaticallyIncludeFirstLevelEntities)
			{
				typeof(TEntityModel)
					.GetProperties()
					.Where(prop => !prop.PropertyType.IsValueType
					               && prop.PropertyType != typeof(string)
					               && (prop.IsVirtual() ?? false))
					.ToList()
					.ForEach(
						prop => query = query.Include(prop.Name));
			}

			if (!string.IsNullOrWhiteSpace(parameters.Id))
				query = query.IdContains<TEntityModel,TId>(parameters.Id);

			if (string.IsNullOrWhiteSpace(parameters.Id) && parameters.Ids.Any())
				query = query.Where(x => parameters.Ids.Contains(x.Id.ToString()));

			if (parameters.DateCreatedBefore.HasValue)
				query = query.CreatedBefore(parameters.DateCreatedBefore.Value, true);

			if (parameters.DateCreatedAfter.HasValue)
				query = query.CreatedAfter(parameters.DateCreatedAfter.Value, true);

			if (parameters.DateModifiedBefore.HasValue)
				query = query.ModifiedBefore(parameters.DateModifiedBefore.Value, true);

			if (parameters.DateModifiedAfter.HasValue)
				query = query.ModifiedAfter(parameters.DateModifiedAfter.Value, true);

			return query;
		}

		protected abstract IQueryable<TEntityModel> BuildQuery(TQueryParameters parameters);

		protected IQueryable<TEntityModel> BuildQueryFinal(
			TQueryParameters parameters,
			IQueryable<TEntityModel> query = null)
		{
			if (query == null)
				query = Context.GetQueryable<TEntityModel>();

			if (parameters == null)
				return query;

			if (!string.IsNullOrWhiteSpace(parameters.SortBy))
				query = query.OrderBy(parameters.SortBy, parameters.SortDirection);

			return query;
		}
	}
}
