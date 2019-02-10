using System;
using System.Linq;
using System.Reflection;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Extensions;
using JDMallen.Toolbox.Infrastructure.EFCore.Filters;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepositoryBase<
		TContext,
		TEntityModel,
		TQueryParameters,
		TId>
		where TContext : DbContext, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters<TId>
		where TId : struct
	{
		protected EFRepositoryBase(TContext context)
		{
			Context = context;
			Set = Context.Set<TEntityModel>();
		}

		protected TContext Context { get; }

		protected DbSet<TEntityModel> Set { get; }

		/// <summary>
		/// https://stackoverflow.com/a/8181736/3986790
		/// </summary>
		/// <param name="oldEntity"></param>
		/// <param name="newEntity"></param>
		private static void CopyProps(
			ref TEntityModel oldEntity,
			ref TEntityModel newEntity)
		{
			TEntityModel old = newEntity;
			TEntityModel newish = oldEntity;
			typeof(TEntityModel)
				.GetFields(
					BindingFlags.Public
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
					.Where(
						prop => !prop.PropertyType.IsValueType
						        && prop.PropertyType != typeof(string)
						        && (prop.IsVirtual() ?? false))
					.ToList()
					.ForEach(
						prop => query = query.Include(prop.Name));
			}

			if (parameters.Id.HasValue)
				query = query.Where(x => x.Id.Equals(parameters.Id));

			if (!parameters.Id.HasValue && parameters.Ids.Any())
				query = query.Where(
					x => parameters.Ids.Contains(x.Id));

			if (parameters.DateCreatedBefore.HasValue)
				query = query.CreatedBefore(
					parameters.DateCreatedBefore.Value,
					true);

			if (parameters.DateCreatedAfter.HasValue)
				query = query.CreatedAfter(
					parameters.DateCreatedAfter.Value,
					true);

			if (parameters.DateModifiedBefore.HasValue)
				query = query.ModifiedBefore(
					parameters.DateModifiedBefore.Value,
					true);

			if (parameters.DateModifiedAfter.HasValue)
				query = query.ModifiedAfter(
					parameters.DateModifiedAfter.Value,
					true);

			query = query.Where(x => x.IsDeleted == parameters.IsDeleted);

			return query;
		}

		protected abstract IQueryable<TEntityModel> BuildQuery(
			TQueryParameters parameters);

		protected IQueryable<TEntityModel> BuildQueryFinal(
			TQueryParameters parameters,
			IQueryable<TEntityModel> query = null)
		{
			if (query == null)
				query = Context.BuildQuery<TEntityModel>();

			if (parameters == null)
				return query;

			if (!string.IsNullOrWhiteSpace(parameters.SortBy))
				query = query.OrderBy(
					parameters.SortBy,
					parameters.SortAscending);

			return query;
		}

		private bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Context.Dispose();
				}
			}

			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
