using JDMallen.Toolbox.Data.Abstractions.Interfaces;

namespace JDMallen.Toolbox.Linq;

/// <summary>
/// Date filters for queries over <see cref="IEntityModel" /> entities.
/// </summary>
public static class EntityModelQueryableExtensions
{
	// Each filter picks its comparison outside the expression rather than
	// embedding an `inclusive ? >= : >` ternary in it, so the SQL is a single
	// comparison.

	/// <param name="query">The query to filter.</param>
	/// <typeparam name="TEntityModel">The entity type.</typeparam>
	extension<TEntityModel>(IQueryable<TEntityModel> query)
		where TEntityModel : IEntityModel
	{
		/// <summary>
		/// Filters to entities created after a date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// True to also include entities created exactly at
		/// <paramref name="dateTime" />.
		/// </param>
		/// <returns>The filtered query.</returns>
		public IQueryable<TEntityModel> CreatedAfter(
			DateTime dateTime,
			bool inclusive = false)
			=> inclusive
				? query.Where(entity => entity.DateCreated >= dateTime)
				: query.Where(entity => entity.DateCreated > dateTime);

		/// <summary>
		/// Filters to entities created before a date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// True to also include entities created exactly at
		/// <paramref name="dateTime" />.
		/// </param>
		/// <returns>The filtered query.</returns>
		public IQueryable<TEntityModel> CreatedBefore(
			DateTime dateTime,
			bool inclusive = false)
			=> inclusive
				? query.Where(entity => entity.DateCreated <= dateTime)
				: query.Where(entity => entity.DateCreated < dateTime);

		/// <summary>
		/// Filters to entities last modified after a date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// True to also include entities modified exactly at
		/// <paramref name="dateTime" />.
		/// </param>
		/// <returns>The filtered query.</returns>
		public IQueryable<TEntityModel> ModifiedAfter(
			DateTime dateTime,
			bool inclusive = false)
			=> inclusive
				? query.Where(entity => entity.DateModified >= dateTime)
				: query.Where(entity => entity.DateModified > dateTime);

		/// <summary>
		/// Filters to entities last modified before a date and time.
		/// </summary>
		/// <param name="dateTime">The date and time to compare against.</param>
		/// <param name="inclusive">
		/// True to also include entities modified exactly at
		/// <paramref name="dateTime" />.
		/// </param>
		/// <returns>The filtered query.</returns>
		public IQueryable<TEntityModel> ModifiedBefore(
			DateTime dateTime,
			bool inclusive = false)
			=> inclusive
				? query.Where(entity => entity.DateModified <= dateTime)
				: query.Where(entity => entity.DateModified < dateTime);
	}
}
