using System.Linq.Expressions;

namespace JDMallen.Toolbox.Extensions
{
	/// <summary>
	/// https://stackoverflow.com/a/37602870/3986790
	/// </summary>
	public class ReplaceVisitor : ExpressionVisitor
	{
		private readonly Expression _from, _to;

		public ReplaceVisitor(Expression from, Expression to)
		{
			_from = from;
			_to = to;
		}

		public override Expression Visit(Expression ex)
		{
			return ex == _from ? _to : base.Visit(ex);
		}
	}
}
