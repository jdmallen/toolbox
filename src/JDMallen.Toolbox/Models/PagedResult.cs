using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JetBrains.Annotations;

namespace JDMallen.Toolbox.Models;

/// <summary>
/// Represents a single page of results from a paginated query.
/// </summary>
/// <typeparam name="TModel">The type of items in the result set.</typeparam>
/// <remarks>
/// <see cref="PageIndex" /> and <see cref="TotalPageCount" /> are calculated values derived from the pagination parameters.
/// </remarks>
[UsedImplicitly]
public class PagedResult<TModel> : IPagedResult<TModel>
	where TModel : class, IModel
{
	/// <summary>
	/// Gets or sets the number of items skipped from the beginning of the result set.
	/// </summary>
	public int Skipped { get; set; }

	/// <summary>
	/// Gets or sets the number of items included in this page.
	/// </summary>
	public int Taken { get; set; }

	/// <summary>
	/// Gets or sets the total number of items across all pages.
	/// </summary>
	public long TotalItemCount { get; set; }

	/// <summary>
	/// Gets the current page number (1-based index).
	/// </summary>
	public int PageIndex
	{
		get
		{
			if (Taken == 0 || Taken > TotalItemCount) return 1;
			return Skipped / Taken + 1;
		}
	}

	/// <summary>
	/// Gets the total number of pages.
	/// </summary>
	public int TotalPageCount
	{
		get
		{
			if (Taken == 0 || Taken > TotalItemCount) return 1;
			return (int)Math.Ceiling((double)TotalItemCount / Taken);
		}
	}

	/// <summary>
	/// Gets or sets the collection of items on this page.
	/// </summary>
	public IEnumerable<TModel> Items { get; set; } = [];
}