using System;

namespace JDMallen.Toolbox.Extensions
{
    public static class StructExtensions
	{
		public static long ToUnixTimestamp(this DateTime dateTime)
			=> (long) Math.Ceiling(dateTime.ToUniversalTime()
											.Subtract(new DateTime(1970, 1, 1))
											.TotalSeconds);

		public static bool IsNullOrWhiteSpace(this string str) 
			=> string.IsNullOrWhiteSpace(str);

		public static bool IsNullOrEmpty(this string str) 
			=> string.IsNullOrEmpty(str);

		public static bool HasValue(this string str)
			=> !string.IsNullOrWhiteSpace(str);
	}
}
