using System.Linq.Expressions;
using JetBrains.Annotations;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for composing and manipulating Expression trees.
/// </summary>
[UsedImplicitly]
public static class ExpressionExtensions
{
	/// <summary>
	/// Composes two expression trees into a single composed expression.
	/// See https://stackoverflow.com/a/37602870/3986790
	/// </summary>
	/// <typeparam name="T">The input type of the first expression.</typeparam>
	/// <typeparam name="TIntermediate">The output type of the first expression and input type of the second.</typeparam>
	/// <typeparam name="TResult">The output type of the second expression.</typeparam>
	/// <param name="first">The first expression to compose.</param>
	/// <param name="second">The second expression to compose.</param>
	/// <returns>A new expression representing the composition of the two expressions.</returns>
	public static Expression<Func<T, TResult>> Compose<
		T, TIntermediate, TResult>(
		this Expression<Func<T, TIntermediate>> first,
		Expression<Func<TIntermediate, TResult>> second)
	{
		return Expression.Lambda<Func<T, TResult>>(
			second.Body.Replace(second.Parameters[0], first.Body),
			first.Parameters[0]);
	}

	/// <summary>
	/// Replaces a specified expression node with another throughout the entire expression tree.
	/// See https://stackoverflow.com/a/37602870/3986790
	/// </summary>
	/// <param name="ex">The expression tree to search.</param>
	/// <param name="from">The expression node to search for.</param>
	/// <param name="to">The expression node to replace it with.</param>
	/// <returns>A new expression tree with the specified node replaced.</returns>
	public static Expression Replace(
		this Expression ex,
		Expression from,
		Expression to)
	{
		// Visit only returns null for null input; ex is non-null here.
		return new ReplaceVisitor(from, to).Visit(ex)!;
	}
}