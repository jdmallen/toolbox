namespace JDMallen.Toolbox.Models;

/// <summary>
/// Defines the search strategy for string matching operations.
/// </summary>
public enum SearchStyle
{
	/// <summary>
	/// Exact match comparison (<c>==</c>), with no case folding.
	/// </summary>
	Exact,

	/// <summary>
	/// Case-insensitive substring match (contains).
	/// </summary>
	Contains,

	/// <summary>
	/// Case-insensitive match at the beginning of the string.
	/// </summary>
	StartsWith,
}
