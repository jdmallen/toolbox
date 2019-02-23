using System;
using System.Linq.Expressions;

namespace JDMallen.Toolbox.Extensions
{
	public static class ExpressionExtensions
	{
		/// <summary>
		/// https://stackoverflow.com/a/37602870/3986790
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TIntermediate"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
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
		/// https://stackoverflow.com/a/37602870/3986790
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static Expression Replace(
			this Expression ex,
			Expression from,
			Expression to)
		{
			return new ReplaceVisitor(from, to).Visit(ex);
		}
	}
}
