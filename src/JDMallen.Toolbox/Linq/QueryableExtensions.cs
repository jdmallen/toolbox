using System.Linq.Expressions;
using System.Reflection;
using JDMallen.Toolbox.Extensions;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.Linq;

/// <summary>
/// Searching and runtime-named sorting for any <see cref="IQueryable{T}" />:
/// LINQ to Objects, or a query provider such as EF Core.
/// </summary>
public static class QueryableExtensions
{
	private static readonly MethodInfo OrderByMethod =
		GetKeySelectorMethod(nameof(Queryable.OrderBy));

	private static readonly MethodInfo OrderByDescendingMethod =
		GetKeySelectorMethod(nameof(Queryable.OrderByDescending));

	// The two-parameter overload, (source, keySelector), as opposed to the one
	// that also takes an IComparer.
	private static MethodInfo GetKeySelectorMethod(string name)
		=> typeof(Queryable)
			.GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Single(method => method.Name == name
				&& method.GetParameters().Length == 2);

	/// <param name="query">The query to filter or sort.</param>
	/// <typeparam name="T">The element type.</typeparam>
	extension<T>(IQueryable<T> query)
	{
		/// <summary>
		/// Filters the query to elements whose string field matches the search
		/// value. The value is trimmed first; a blank value returns the query
		/// unchanged.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="SearchStyle.Contains" /> and
		/// <see cref="SearchStyle.StartsWith" /> ignore case. Both sides are
		/// upper-cased (<c>UPPER()</c> in SQL), because query providers don't
		/// translate the <see cref="StringComparison" /> overloads. That also
		/// means a database index can't serve a <c>StartsWith</c> search.
		/// Non-ASCII case folding depends on the provider (SQLite's
		/// <c>upper()</c> only folds ASCII).
		/// </para>
		/// <para>
		/// <see cref="SearchStyle.Exact" /> is a plain <c>==</c>: ordinal in
		/// memory, and governed by the column's collation in a database.
		/// </para>
		/// </remarks>
		/// <param name="field">Selects the string field to search.</param>
		/// <param name="searchValue">The value to search for.</param>
		/// <param name="searchStyle">How the field must match the value.</param>
		/// <returns>The filtered query.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="searchStyle" /> is not a defined
		/// <see cref="SearchStyle" />.
		/// </exception>
		public IQueryable<T> Search(
			Expression<Func<T, string>> field,
			string? searchValue,
			SearchStyle searchStyle = SearchStyle.Exact)
		{
			if (string.IsNullOrWhiteSpace(searchValue))
			{
				return query;
			}

			string trimmed = searchValue!.Trim();
			string upper = trimmed.ToUpperInvariant();

			// The culture-less ToUpper, Contains, and StartsWith overloads are the
			// ones query providers translate; the analyzer-preferred overloads
			// with a CultureInfo or StringComparison throw at query time.
#pragma warning disable CA1304, CA1307, CA1310, CA1311, CA1862
			return searchStyle switch
			{
				SearchStyle.Exact => query.Where(
					field.Compose(value => value == trimmed)),
				SearchStyle.Contains => query.Where(
					field.Compose(value => value.ToUpper().Contains(upper))),
				SearchStyle.StartsWith => query.Where(
					field.Compose(value => value.ToUpper().StartsWith(upper))),
				_ => throw new ArgumentOutOfRangeException(
					nameof(searchStyle),
					searchStyle,
					null),
			};
#pragma warning restore CA1304, CA1307, CA1310, CA1311, CA1862
		}

		/// <summary>
		/// Sorts the query by a property named at runtime. Nested properties use
		/// dot notation, such as <c>"Address.City"</c>. A blank path returns the
		/// query unchanged.
		/// </summary>
		/// <param name="propertyPath">
		/// A property or field name, or a dot-separated path of them.
		/// </param>
		/// <param name="ascending">
		/// True to sort ascending; false to sort descending.
		/// </param>
		/// <returns>The sorted query.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="propertyPath" /> names a member that does not exist.
		/// </exception>
		public IQueryable<T> OrderByProperty(
			string? propertyPath,
			bool ascending = true)
		{
			if (string.IsNullOrWhiteSpace(propertyPath))
			{
				return query;
			}

			LambdaExpression keySelector =
				PropertyPath.ToLambda(typeof(T), propertyPath!);

			MethodInfo orderBy =
				(ascending ? OrderByMethod : OrderByDescendingMethod)
				.MakeGenericMethod(typeof(T), keySelector.ReturnType);

			return query.Provider.CreateQuery<T>(
				Expression.Call(
					null,
					orderBy,
					query.Expression,
					Expression.Quote(keySelector)));
		}
	}
}
