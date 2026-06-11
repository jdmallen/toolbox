using System.Linq.Expressions;
using JetBrains.Annotations;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Expression visitor that replaces one expression node with another throughout the expression tree.
/// See https://stackoverflow.com/a/37602870/3986790
/// </summary>
[UsedImplicitly]
public class ReplaceVisitor : ExpressionVisitor
{
	private readonly Expression _from, _to;

	/// <summary>
	/// Initializes a new instance of the <see cref="ReplaceVisitor"/> class.
	/// </summary>
	/// <param name="from">The expression node to search for.</param>
	/// <param name="to">The expression node to replace it with.</param>
	public ReplaceVisitor(Expression from, Expression to)
	{
		_from = from;
		_to = to;
	}

	/// <summary>
	/// Visits the specified expression node and replaces it if it matches the target expression.
	/// </summary>
	/// <param name="ex">The expression node to visit.</param>
	/// <returns>The replacement expression if it matches, otherwise the result of visiting the expression normally.</returns>
	public override Expression? Visit(Expression? ex)
	{
		return ex == _from ? _to : base.Visit(ex);
	}
}