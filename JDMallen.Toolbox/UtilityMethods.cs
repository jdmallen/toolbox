using System;
using System.Linq;
using System.Linq.Expressions;

namespace JDMallen.Toolbox
{
    public static class UtilityMethods
    {
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
	}
}
