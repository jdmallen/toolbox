using System.Diagnostics.CodeAnalysis;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Instance-style wrappers for the static <see cref="string" /> null and blank
/// checks, so <c>value.IsNullOrWhiteSpace()</c> reads left to right. They
/// accept a null receiver, and on .NET Core 3.0+ they carry the same
/// <c>NotNullWhen</c> annotations as the BCL methods, so the compiler knows the
/// value is not null after a successful check.
/// </summary>
[SuppressMessage(
	"Naming",
	"CA1708:Identifiers should differ by more than case",
	Justification =
		"False positive on C# 14 extension blocks: CA1708 compares the compiler-"
		+ "generated grouping types for the two extension(string?) blocks. The "
		+ "public extension methods themselves are uniquely named.")]
public static class StringExtensions
{
	// A receiver attribute applies to every member in its extension block, so
	// HasValue (not null when true) and the other two (not null when false) need
	// separate blocks. NotNullWhenAttribute doesn't exist on netstandard2.0.

	/// <param name="str">The string to test.</param>
#if NETCOREAPP3_0_OR_GREATER
	extension([NotNullWhen(true)] string? str)
#else
	extension(string? str)
#endif
	{
		/// <summary>
		/// Determines whether the string is not null and contains at least one
		/// non-white-space character.
		/// </summary>
		/// <returns>
		/// true if the string is not null, empty, or white space; otherwise,
		/// false.
		/// </returns>
		public bool HasValue() => !string.IsNullOrWhiteSpace(str);
	}

	/// <param name="str">The string to test.</param>
#if NETCOREAPP3_0_OR_GREATER
	extension([NotNullWhen(false)] string? str)
#else
	extension(string? str)
#endif
	{
		/// <summary>
		/// Determines whether the string is null or empty.
		/// </summary>
		/// <returns>
		/// true if the string is null or an empty string (""); otherwise, false.
		/// </returns>
		public bool IsNullOrEmpty() => string.IsNullOrEmpty(str);

		/// <summary>
		/// Determines whether the string is null, empty, or consists only of
		/// white-space characters.
		/// </summary>
		/// <returns>
		/// true if the string is null, empty, or white space; otherwise, false.
		/// </returns>
		public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(str);
	}
}
