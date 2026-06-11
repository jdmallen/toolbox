namespace JDMallen.Toolbox.Models;

/// <summary>
/// Defines the search strategy for string matching operations.
/// </summary>
public enum SearchStyle
{
	/// <summary>
	/// Exact match comparison.
	/// </summary>
	Exact,

	/// <summary>
	/// Case-insensitive substring match (contains).
	/// </summary>
	Contains,

	/// <summary>
	/// Match at the beginning of the string.
	/// </summary>
	StartsWith,
}
