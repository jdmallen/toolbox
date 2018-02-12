using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

		    var lambda = (dynamic)GetExpressionFromPropertyName(typeof(TSource), fieldName);

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

		/// <summary>
		/// https://stackoverflow.com/a/16208620/3986790
		/// </summary>
		/// <param name="type"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static LambdaExpression GetExpressionFromPropertyName(Type type, string propertyName)
	    {
		    var param = Expression.Parameter(type, "x");

		    var body = propertyName
			    .Split('.')
			    .Aggregate<string, Expression>(param, Expression.PropertyOrField);

		    return Expression.Lambda(body, param);
	    }

	    /// <summary>
	    /// https://stackoverflow.com/a/4243560/3986790
	    /// </summary>
	    /// <param name="self"></param>
	    /// <returns></returns>
	    public static bool? IsVirtual(this PropertyInfo self)
	    {
		    if (self == null)
			    throw new ArgumentNullException(nameof(self));

		    bool? found = null;

		    foreach (var method in self.GetAccessors())
		    {
			    if (found.HasValue)
			    {
				    if (found.Value != method.IsVirtual)
					    return null;
			    }
			    else
			    {
				    found = method.IsVirtual;
			    }
		    }

		    return found;
	    }
	}
}
