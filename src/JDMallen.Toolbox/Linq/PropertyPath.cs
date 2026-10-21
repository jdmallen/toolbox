using System.Linq.Expressions;

namespace JDMallen.Toolbox.Linq;

/// <summary>
/// Builds member-access lambdas from property names, for sorting or projecting
/// by a name that is only known at runtime.
/// </summary>
public static class PropertyPath
{
	/// <summary>
	/// Creates a lambda that reads a property or field by name. Nested members
	/// use dot notation, such as <c>"Address.City"</c>.
	/// </summary>
	/// <param name="type">The lambda's parameter type.</param>
	/// <param name="propertyPath">
	/// A property or field name, or a dot-separated path of them.
	/// </param>
	/// <returns>
	/// A lambda of the form <c>x =&gt; x.Property</c> or
	/// <c>x =&gt; x.Parent.Property</c>, typed by the last member.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="type" /> is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="propertyPath" /> is blank, or names a member that does not
	/// exist.
	/// </exception>
	public static LambdaExpression ToLambda(Type type, string propertyPath)
	{
#pragma warning disable CA1510 // netstandard2.0 doesn't have ThrowIfNull
		if (type is null)
#pragma warning restore CA1510
		{
			throw new ArgumentNullException(nameof(type));
		}

		if (string.IsNullOrWhiteSpace(propertyPath))
		{
			throw new ArgumentException(
				"Property path cannot be null or whitespace.",
				nameof(propertyPath));
		}

		ParameterExpression parameter = Expression.Parameter(type, "x");

		Expression body = propertyPath
			.Split('.')
			.Aggregate<string, Expression>(parameter, Expression.PropertyOrField);

		return Expression.Lambda(body, parameter);
	}

	/// <summary>
	/// Creates a lambda that reads a property or field of
	/// <typeparamref name="T" /> by name.
	/// </summary>
	/// <typeparam name="T">The lambda's parameter type.</typeparam>
	/// <param name="propertyPath">
	/// A property or field name, or a dot-separated path of them.
	/// </param>
	/// <returns>A lambda of the form <c>x =&gt; x.Property</c>.</returns>
	/// <exception cref="ArgumentException">
	/// <paramref name="propertyPath" /> is blank, or names a member that does not
	/// exist.
	/// </exception>
	public static LambdaExpression ToLambda<T>(string propertyPath)
		=> ToLambda(typeof(T), propertyPath);
}
