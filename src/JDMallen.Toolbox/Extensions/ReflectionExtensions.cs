using System.Reflection;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for reflection operations.
/// </summary>
public static class ReflectionExtensions
{
	/// <summary>
	/// Determines whether the given property is virtual.
	/// If the property has both getter and setter, they must both be virtual for this
	/// to return true.
	/// See https://stackoverflow.com/a/4243560/3986790
	/// </summary>
	/// <param name="self">The property to check.</param>
	/// <returns>
	/// true if the property is virtual; false if not virtual; null if the
	/// property has conflicting virtual modifiers on getter/setter.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown if <paramref name="self" /> is
	/// null.
	/// </exception>
	public static bool? IsVirtual(this PropertyInfo self)
	{
#pragma warning disable CA1510 // netstandard2.0 doesn't have ThrowIfNull
		if (self is null)
#pragma warning restore CA1510
		{
			throw new ArgumentNullException(nameof(self));
		}

		bool? found = null;

		foreach (MethodInfo method in self.GetAccessors())
		{
			if (found.HasValue)
			{
				if (found.Value != method.IsVirtual)
				{
					return null;
				}
			}
			else
			{
				found = method.IsVirtual;
			}
		}

		return found;
	}
}
