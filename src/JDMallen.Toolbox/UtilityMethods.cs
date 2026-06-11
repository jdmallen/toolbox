using System.Linq.Expressions;

namespace JDMallen.Toolbox;

/// <summary>
/// Utility methods for common operations.
/// </summary>
public static class UtilityMethods
{
	/// <summary>
	/// Creates a lambda expression that accesses a property by name on a given type.
	/// Supports nested properties using dot notation (e.g., "Address.City").
	/// See https://stackoverflow.com/a/16208620/3986790
	/// </summary>
	/// <param name="type">The type to create the parameter for.</param>
	/// <param name="propertyName">
	/// The property name or dot-separated property path to
	/// access.
	/// </param>
	/// <returns>
	/// A lambda expression of the form <c>x => x.Property</c> or
	/// <c>x => x.Parent.Property</c>.
	/// </returns>
	public static LambdaExpression GetExpressionFromPropertyName(Type type, string propertyName)
	{
		ParameterExpression param = Expression.Parameter(type, "x");

		Expression body = propertyName
			.Split('.')
			.Aggregate<string, Expression>(param, Expression.PropertyOrField);

		return Expression.Lambda(body, param);
	}
}
