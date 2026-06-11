namespace JDMallen.Toolbox.EFCore.Extensions;

/// <summary>
/// Extension methods for IQueryable to provide dynamic ordering capabilities.
/// </summary>
public static class QueryExtensions
{
	/// <summary>
	/// https://stackoverflow.com/a/40572006/3986790
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <param name="query"></param>
	/// <param name="fieldName"></param>
	/// <param name="ascending"></param>
	/// <returns></returns>
	public static IQueryable<TSource> OrderBy<TSource>(
		this IQueryable<TSource> query,
		string fieldName,
		bool ascending)
	{
		if (string.IsNullOrWhiteSpace(fieldName))
		{
			return query;
		}

		dynamic lambda =
			UtilityMethods.GetExpressionFromPropertyName(
				typeof(TSource),
				fieldName);

		return ascending
			? Queryable.OrderBy(query, lambda)
			: Queryable.OrderByDescending(query, lambda);
	}
}
