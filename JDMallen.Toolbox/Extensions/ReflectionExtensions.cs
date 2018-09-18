using System;
using System.Reflection;

namespace JDMallen.Toolbox.Extensions
{
	public static class ReflectionExtensions
	{
		/// <summary>
		/// https://stackoverflow.com/a/4243560/3986790
		/// </summary>
		/// <param name="self"></param>
		/// <returns></returns>
		public static bool? IsVirtual(this PropertyInfo self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			bool? found = null;

			foreach (var method in self.GetAccessors())
			{
				if (found.HasValue)
				{
					if (found.Value != method.IsVirtual)
						return null;
				}
				else
				{
					found = method.IsVirtual;
				}
			}

			return found;
		}
	}
}
