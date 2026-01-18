using System.Linq.Expressions;

namespace JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;

/// <summary>
/// Defines a specification pattern for querying entities with criteria, includes,
/// ordering, and paging.
/// See https://deviq.com/specification-pattern/ for more information.
/// </summary>
/// <typeparam name="T">The entity type this specification applies to.</typeparam>
public interface ISpecification<T>
{
	/// <summary>
	/// Gets the filter criteria expression to apply to the query.
	/// </summary>
	Expression<Func<T, bool>> Criteria { get; }

	/// <summary>
	/// Gets the collection of related entities to eagerly load using expression-based
	/// includes.
	/// </summary>
	IEnumerable<Expression<Func<T, object>>> Includes { get; }

	/// <summary>
	/// Gets the collection of related entities to eagerly load using string-based
	/// navigation paths.
	/// </summary>
	IEnumerable<string> IncludeStrings { get; }

	/// <summary>
	/// Gets the expression to use for ordering results in ascending order.
	/// </summary>
	Expression<Func<T, object>>? OrderBy { get; }

	/// <summary>
	/// Gets the expression to use for ordering results in descending order.
	/// </summary>
	Expression<Func<T, object>>? OrderByDescending { get; }

	/// <summary>
	/// Gets the maximum number of records to retrieve when paging is enabled.
	/// </summary>
	int? Take { get; }

	/// <summary>
	/// Gets the number of records to skip when paging is enabled.
	/// </summary>
	int? Skip { get; }

	/// <summary>
	/// Gets a value indicating whether pagination should be applied to the query.
	/// </summary>
	bool IsPagingEnabled { get; }
}
