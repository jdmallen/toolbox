namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for struct types and related conversions.
/// </summary>
public static class StructExtensions
{
	/// <summary>
	/// Converts a DateTime to its Unix timestamp representation (seconds since epoch).
	/// </summary>
	/// <param name="dateTime">The DateTime value to convert.</param>
	/// <returns>The number of seconds since the Unix epoch (January 1, 1970).</returns>
	public static long ToUnixTimestamp(this DateTime dateTime) => (long)Math.Ceiling(
		dateTime.ToUniversalTime()
			.Subtract(new DateTime(1970, 1, 1))
			.TotalSeconds);

	/// <param name="str">The string to test.</param>
	extension(string str)
	{
		/// <summary>
		/// Determines whether the specified string is not null and not composed entirely
		/// of white-space characters.
		/// </summary>
		/// <returns>
		/// true if the value parameter is not null and contains at least one
		/// non-white-space character; otherwise, false.
		/// </returns>
		public bool HasValue() => !string.IsNullOrWhiteSpace(str);

		/// <summary>
		/// Determines whether the specified string is null or empty.
		/// </summary>
		/// <returns>
		/// true if the value parameter is null or an empty string ("");
		/// otherwise, false.
		/// </returns>
		public bool IsNullOrEmpty() => string.IsNullOrEmpty(str);

		/// <summary>
		/// Determines whether the specified string is null, empty, or consists only of
		/// white-space characters.
		/// </summary>
		/// <returns>
		/// true if the value parameter is null or empty, or if value consists
		/// exclusively of white-space characters; otherwise, false.
		/// </returns>
		public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(str);
	}
}
