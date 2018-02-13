using System.ComponentModel;
using System.Linq;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Extensions
{
    public static class QueryExtensions
    {
		/// <summary>
		/// https://stackoverflow.com/a/40572006/3986790
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="query"></param>
		/// <param name="fieldName"></param>
		/// <param name="sortDirection"></param>
		/// <returns></returns>
		public static IQueryable<TSource> OrderBy<TSource>(
		    this IQueryable<TSource> query,
		    string fieldName,
		    ListSortDirection sortDirection = ListSortDirection.Ascending)
	    {
		    if (string.IsNullOrWhiteSpace(fieldName))
		    {
			    return query;
		    }

		    var lambda =
			    (dynamic) UtilityMethods.GetExpressionFromPropertyName(typeof(TSource), fieldName);

		    // ReSharper disable once RedundantCaseLabel
		    switch (sortDirection)
		    {
			    case ListSortDirection.Descending:
				    return Queryable.OrderByDescending(query, lambda);
			    case ListSortDirection.Ascending:
			    default:
				    return Queryable.OrderBy(query, lambda);
		    }
	    }
	}
}
