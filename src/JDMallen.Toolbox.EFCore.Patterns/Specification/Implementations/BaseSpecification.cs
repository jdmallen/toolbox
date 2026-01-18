using System.Linq.Expressions;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Specification.Implementations;

/// <summary>
/// https://deviq.com/specification-pattern/
/// </summary>
public class BaseSpecification<T>(Expression<Func<T, bool>> criteria)
	: ISpecification<T>
{
	private readonly List<Expression<Func<T, object>>> _includes = [];
	private readonly List<string> _includeStrings = [];

	/// <inheritdoc />
	public int? Take { get; private set; }

	/// <inheritdoc />
	public int? Skip { get; private set; }

	/// <inheritdoc />
	public bool IsPagingEnabled { get; private set; }

	/// <inheritdoc />
	public Expression<Func<T, object>>? OrderBy { get; private set; }

	/// <inheritdoc />
	public Expression<Func<T, object>>? OrderByDescending { get; private set; }

	/// <inheritdoc />
	public Expression<Func<T, bool>> Criteria { get; } = criteria;

	/// <inheritdoc />
	public IEnumerable<Expression<Func<T, object>>> Includes =>
		_includes.AsReadOnly();

	/// <inheritdoc />
	public IEnumerable<string> IncludeStrings => _includeStrings.AsReadOnly();

	/// <summary>
	/// Adds an include expression for eager-loading related entities.
	/// </summary>
	/// <remarks>
	/// This method allows derived specifications to define which related entities
	/// should be loaded from the database along with the main entity.
	/// </remarks>
	/// <param name="includeExpression">
	/// A lambda expression that specifies the navigation property to include.
	/// Example: x => x.Orders
	/// </param>
	protected virtual void AddInclude(
		Expression<Func<T, object>> includeExpression)
	{
		_includes.Add(includeExpression);
	}

	/// <summary>
	/// Adds a string-based include for eager-loading nested related entities.
	/// </summary>
	/// <remarks>
	/// String-based includes allow for including children of children,
	/// e.g. "Orders.Items" or "Orders.Items.Product".
	/// This is useful for complex navigation paths that cannot be easily expressed
	/// with lambda expressions.
	/// </remarks>
	/// <param name="includeString">
	/// A dot-separated string path representing nested navigation properties.
	/// Example: "Orders.Items.Product"
	/// </param>
	protected virtual void AddInclude(string includeString)
	{
		_includeStrings.Add(includeString);
	}

	/// <summary>
	/// Applies pagination parameters to the specification.
	/// </summary>
	/// <remarks>
	/// When applied, the query will skip the specified number of records and take only
	/// the specified number of records, enabling result paging.
	/// </remarks>
	/// <param name="skip">The number of records to skip (zero-based offset).</param>
	/// <param name="take">The maximum number of records to retrieve.</param>
	protected virtual void ApplyPaging(int skip, int take)
	{
		Skip = skip;
		Take = take;
		IsPagingEnabled = true;
	}

	/// <summary>
	/// Applies an ascending order expression to the specification.
	/// </summary>
	/// <remarks>
	/// Derived specifications can call this method to sort results in ascending order
	/// by a specific property or expression.
	/// </remarks>
	/// <param name="orderByExpression">
	/// A lambda expression that specifies the property to sort by.
	/// Example: x => x.Name or x => x.CreatedDate
	/// </param>
	protected virtual void ApplyOrderBy(
		Expression<Func<T, object>> orderByExpression)
	{
		OrderBy = orderByExpression;
	}

	/// <summary>
	/// Applies a descending order expression to the specification.
	/// </summary>
	/// <remarks>
	/// Derived specifications can call this method to sort results in descending order
	/// by a specific property or expression.
	/// </remarks>
	/// <param name="orderByDescendingExpression">
	/// A lambda expression that specifies the property to sort by.
	/// Example: x => x.CreatedDate
	/// </param>
	protected virtual void ApplyOrderByDescending(
		Expression<Func<T, object>> orderByDescendingExpression)
	{
		OrderByDescending = orderByDescendingExpression;
	}
}
