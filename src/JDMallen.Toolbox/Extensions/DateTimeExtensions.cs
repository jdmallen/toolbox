using JetBrains.Annotations;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for DateTime conversions with Unix timestamp representations.
/// </summary>
[UsedImplicitly]
public static class DateTimeExtensions
{
	private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// Converts a DateTime to its Unix timestamp in milliseconds.
	/// </summary>
	/// <param name="dateTime">The DateTime value to convert.</param>
	/// <returns>The number of milliseconds since the Unix epoch (January 1, 1970).</returns>
	public static long ToUnixTimeMillis(this DateTime dateTime)
	{
		return (long)(dateTime - Epoch).TotalMilliseconds;
	}

	/// <summary>
	/// Converts a Unix timestamp in milliseconds back to a DateTime.
	/// </summary>
	/// <param name="unixTimeMillis">The number of milliseconds since the Unix epoch.</param>
	/// <returns>The corresponding DateTime in UTC.</returns>
	public static DateTime FromUnixTimeMillis(this long unixTimeMillis)
	{
		return Epoch.AddMilliseconds(unixTimeMillis);
	}

	/// <summary>
	/// Converts a DateTime to its Unix timestamp in seconds.
	/// </summary>
	/// <param name="dateTime">The DateTime value to convert.</param>
	/// <returns>The number of seconds since the Unix epoch (January 1, 1970).</returns>
	public static long ToUnixTime(this DateTime dateTime)
	{
		return (long)(dateTime - Epoch).TotalSeconds;
	}

	/// <summary>
	/// Converts a Unix timestamp in seconds back to a DateTime.
	/// </summary>
	/// <param name="unixTime">The number of seconds since the Unix epoch.</param>
	/// <returns>The corresponding DateTime in UTC.</returns>
	public static DateTime FromUnixTime(this long unixTime)
	{
		return Epoch.AddSeconds(unixTime);
	}
}