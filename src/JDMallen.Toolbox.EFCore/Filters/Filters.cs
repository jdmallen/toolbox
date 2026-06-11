using System.Linq.Expressions;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.EFCore.Filters;

/// <summary>
/// Extension methods for filtering IQueryable entity collections by date and other
/// criteria.
/// </summary>
public static class Filters
{
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
		{
			return query;
		}

		searchValue = searchValue.Trim();

		return searchStyle switch
		{
			SearchStyle.Exact => query.Where(
				fieldExpression.Compose(field => field == searchValue)),
			SearchStyle.Contains => query.Where(
				fieldExpression.Compose(field => field.Contains(
					searchValue,
					StringComparison.CurrentCultureIgnoreCase))),
			SearchStyle.StartsWith => query.Where(
				fieldExpression.Compose(field => field.StartsWith(
					searchValue,
					StringComparison.CurrentCultureIgnoreCase))),
			_ => throw new ArgumentOutOfRangeException(
				nameof(searchStyle),
				searchStyle,
				null),
		};
	}

	/// <param name="query">The query to filter.</param>
	/// <typeparam name="TEntityModel">The entity type.</typeparam>
	extension<TEntityModel>(IQueryable<TEntityModel> query)
		where TEntityModel : IEntityModel
	{
		/// <summary>
		/// Filters entities created after a specified date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// If true, includes entities created exactly at the
		/// specified time.
		/// </param>
		/// <returns>A filtered queryable of entities.</returns>
		public IQueryable<TEntityModel> CreatedAfter(
			DateTime dateTime,
			bool inclusive = false)
		{
			return query.Where(e => inclusive
				? e.DateCreated >= dateTime
				: e.DateCreated > dateTime);
		}

		/// <summary>
		/// Filters entities created before a specified date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// If true, includes entities created exactly at the
		/// specified time.
		/// </param>
		/// <returns>A filtered queryable of entities.</returns>
		public IQueryable<TEntityModel> CreatedBefore(
			DateTime dateTime,
			bool inclusive = false)
		{
			return query.Where(e => inclusive
				? e.DateCreated <= dateTime
				: e.DateCreated < dateTime);
		}

		/// <summary>
		/// Filters entities modified after a specified date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// If true, includes entities modified exactly at the
		/// specified time.
		/// </param>
		/// <returns>A filtered queryable of entities.</returns>
		public IQueryable<TEntityModel> ModifiedAfter(
			DateTime dateTime,
			bool inclusive = false)
		{
			return query.Where(e => inclusive
				? e.DateModified >= dateTime
				: e.DateModified > dateTime);
		}

		/// <summary>
		/// Filters entities modified before a specified date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// If true, includes entities modified exactly at the
		/// specified time.
		/// </param>
		/// <returns>A filtered queryable of entities.</returns>
		public IQueryable<TEntityModel> ModifiedBefore(
			DateTime dateTime,
			bool inclusive = false)
		{
			return query.Where(e => inclusive
				? e.DateModified <= dateTime
				: e.DateModified < dateTime);
		}
	}
}
