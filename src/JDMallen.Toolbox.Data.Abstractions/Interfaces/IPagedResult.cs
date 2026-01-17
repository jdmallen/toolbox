namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Represents a result that has been divided into pages for ease of displaying
/// to end users.
/// </summary>
/// <typeparam name="TModel">The model type contained in the paged result.</typeparam>
public interface IPagedResult<TModel> where TModel : IModel
{
	/// <summary>
	/// Gets or sets the number of items that were skipped to reach this page.
	/// </summary>
	int Skipped { get; set; }

	/// <summary>
	/// Gets or sets the number of items included in this page.
	/// </summary>
	int Taken { get; set; }

	/// <summary>
	/// Gets or sets the total number of items across all pages.
	/// </summary>
	long TotalItemCount { get; set; }

	/// <summary>
	/// Gets the zero-based index of the current page.
	/// </summary>
	int PageIndex { get; }

	/// <summary>
	/// Gets the total number of pages available.
	/// </summary>
	int TotalPageCount { get; }

	/// <summary>
	/// Gets or sets the collection of items in the current page.
	/// </summary>
	IEnumerable<TModel> Items { get; set; }
}
