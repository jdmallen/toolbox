using System;
using System.Linq;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Filters
{
	public static class Filters
	{
		public static IQueryable<TEntityModel> CreatedBefore<TEntityModel>(
			this IQueryable<TEntityModel> query,
			DateTime dateTime,
			bool inclusive = false)
			where TEntityModel : IEntityModel
		{
			return query.Where(
				e => inclusive
					? e.DateCreated <= dateTime
					: e.DateCreated < dateTime);
		}

		public static IQueryable<TEntityModel> CreatedAfter<TEntityModel>(
			this IQueryable<TEntityModel> query,
			DateTime dateTime,
			bool inclusive = false)
			where TEntityModel : IEntityModel
		{
			return query.Where(
				e => inclusive
					? e.DateCreated >= dateTime
					: e.DateCreated > dateTime);
		}

		public static IQueryable<TEntityModel> ModifiedBefore<TEntityModel>(
			this IQueryable<TEntityModel> query,
			DateTime dateTime,
			bool inclusive = false)
			where TEntityModel : IEntityModel
		{
			return query.Where(
				e => inclusive
					? e.DateModified <= dateTime
					: e.DateModified < dateTime);
		}

		public static IQueryable<TEntityModel> ModifiedAfter<TEntityModel>(
			this IQueryable<TEntityModel> query,
			DateTime dateTime,
			bool inclusive = false)
			where TEntityModel : IEntityModel
		{
			return query.Where(
				e => inclusive
					? e.DateModified >= dateTime
					: e.DateModified > dateTime);
		}
	}
}
