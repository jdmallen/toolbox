using System;
using System.Linq;
using System.Linq.Expressions;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;

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

		/// <summary>
		/// https://stackoverflow.com/a/53622600/3986790
		/// </summary>
		/// <typeparam name="TEntityModel"></typeparam>
		/// <param name="query"></param>
		/// <param name="fieldExpression"></param>
		/// <param name="searchValue"></param>
		/// <param name="searchStyle"></param>
		/// <returns></returns>
		public static IQueryable<TEntityModel> Search<TEntityModel>(
			this IQueryable<TEntityModel> query,
			Expression<Func<TEntityModel, string>> fieldExpression,
			string searchValue,
			SearchStyle searchStyle = SearchStyle.Exact)
		{
			if (string.IsNullOrWhiteSpace(searchValue))
				return query;

			searchValue = searchValue.Trim();

			switch (searchStyle)
			{
				case SearchStyle.Exact:
					return query.Where(
						fieldExpression.Compose(field => field == searchValue));
				case SearchStyle.Contains:
					return query.Where(
						fieldExpression.Compose(
							field => field.ToLower()
								.Contains(searchValue.ToLower())));
				case SearchStyle.StartsWith:
					return query.Where(
						fieldExpression.Compose(
							field => field.ToLower()
								.StartsWith(searchValue.ToLower())));
				default:
					throw new ArgumentOutOfRangeException(
						nameof(searchStyle),
						searchStyle,
						null);
			}
		}
	}
}
