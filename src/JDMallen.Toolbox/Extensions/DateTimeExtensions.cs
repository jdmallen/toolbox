using System.Diagnostics.CodeAnalysis;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for DateTime conversions with Unix timestamp representations.
/// </summary>
[SuppressMessage(
	"Naming",
	"CA1708:Identifiers should differ by more than case",
	Justification =
		"False positive on C# 14 extension blocks: CA1708 compares the compiler-"
		+ "generated grouping types for extension(long)/extension(DateTime). The "
		+ "public extension methods themselves are uniquely named.")]
public static class DateTimeExtensions
{
	// @formatter:off
	private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	// @formatter:on

	/// <param name="unixTime">The number of seconds since the Unix epoch.</param>
	extension(long unixTime)
	{
		/// <summary>
		/// Converts a Unix timestamp in seconds back to a DateTime.
		/// </summary>
		/// <returns>The corresponding DateTime in UTC.</returns>
		public DateTime FromUnixTime() => Epoch.AddSeconds(unixTime);

		/// <summary>
		/// Converts a Unix timestamp in milliseconds back to a DateTime.
		/// </summary>
		/// <returns>The corresponding DateTime in UTC.</returns>
		public DateTime FromUnixTimeMillis()
			=> Epoch.AddMilliseconds(unixTime);
	}

	/// <param name="dateTime">The DateTime value to convert.</param>
	extension(DateTime dateTime)
	{
		/// <summary>
		/// Converts a DateTime to its Unix timestamp in seconds.
		/// </summary>
		/// <returns>The number of seconds since the Unix epoch (January 1, 1970).</returns>
		public long ToUnixTime() => (long)(dateTime - Epoch).TotalSeconds;

		/// <summary>
		/// Converts a DateTime to its Unix timestamp in milliseconds.
		/// </summary>
		/// <returns>The number of milliseconds since the Unix epoch (January 1, 1970).</returns>
		public long ToUnixTimeMillis()
			=> (long)(dateTime - Epoch).TotalMilliseconds;
	}
}
