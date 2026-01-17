using System;

namespace JDMallen.Toolbox.Extensions;

public static class DateTimeExtensions
{
	private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static long ToUnixTimeMillis(this DateTime dateTime)
	{
		return (long)(dateTime - Epoch).TotalMilliseconds;
	}

	public static DateTime FromUnixTimeMillis(this long unixTimeMillis)
	{
		return Epoch.AddMilliseconds(unixTimeMillis);
	}

	public static long ToUnixTime(this DateTime dateTime)
	{
		return (long)(dateTime - Epoch).TotalSeconds;
	}

	public static DateTime FromUnixTime(this long unixTime)
	{
		return Epoch.AddSeconds(unixTime);
	}
}