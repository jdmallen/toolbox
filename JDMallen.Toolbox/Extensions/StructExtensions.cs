using System;

namespace JDMallen.Toolbox.Extensions
{
    public static class StructExtensions
	{
		public static long ToUnixTimestamp(this DateTime dateTime)
			=> (long) Math.Ceiling(dateTime.ToUniversalTime()
											.Subtract(new DateTime(1970, 1, 1))
											.TotalSeconds);
	}
}
