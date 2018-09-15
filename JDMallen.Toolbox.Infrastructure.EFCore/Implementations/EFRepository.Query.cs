using System.Linq;
using System.Reflection;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Filters;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TQueryParameters, TId>
		: IRepository
		where TContext : class, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		protected EFRepositoryBase(TContext context)
		{
			Context = context;
		}

		protected TContext Context { get; }

		/// <summary>
		/// https://stackoverflow.com/a/8181736/3986790
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="newEntity"></param>
		private static void CopyProps(ref TEntityModel oldEntity, ref TEntityModel newEntity)
		{
			TEntityModel old = newEntity;
			TEntityModel newish = oldEntity;
			typeof(TEntityModel)
				.GetFields(BindingFlags.Public
				           | BindingFlags.Instance)
				.ToList()
				.ForEach(field => field.SetValue(old, field.GetValue(newish)));
			oldEntity = old;
			newEntity = newish;
		}

		protected IQueryable<TEntityModel> BuildQueryInit(
			TQueryParameters parameters,
			IQueryable<TEntityModel> query = null)
		{
			if (query == null)
				query = Context.BuildQuery<TEntityModel>();

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
				query = query.IdContains<TEntityModel, TId>(parameters.Id);

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
				query = Context.BuildQuery<TEntityModel>();

			if (parameters == null)
				return query;

			if (!string.IsNullOrWhiteSpace(parameters.SortBy))
				query = query.OrderBy(parameters.SortBy, parameters.SortAscending);

			return query;
		}
	}
}
