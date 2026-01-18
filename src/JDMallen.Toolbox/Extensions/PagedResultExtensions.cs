using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.Models;
using JetBrains.Annotations;

namespace JDMallen.Toolbox.Extensions;

/// <summary>
/// Extension methods for working with paged results.
/// </summary>
[UsedImplicitly]
public static class PagedResultExtensions
{
	/// <summary>
	/// Wraps an enumerable collection of items into a <see cref="PagedResult{TModel}"/>.
	/// </summary>
	/// <typeparam name="TModel">The type of items in the collection.</typeparam>
	/// <param name="models">The collection of items in this page.</param>
	/// <param name="skipped">The number of items skipped from the beginning (normalized to 0 if negative).</param>
	/// <param name="taken">The number of items in this page (defaulting to total if negative).</param>
	/// <param name="total">The total number of items across all pages.</param>
	/// <returns>A <see cref="PagedResult{TModel}"/> containing the provided items and pagination information.</returns>
	public static PagedResult<TModel> AsPaged<TModel>(
		this IEnumerable<TModel> models,
		int skipped,
		int taken,
		long total)
		where TModel : class, IModel
	{
		return new PagedResult<TModel>
		{
			Items = models,
			Skipped = skipped < 0 ? 0 : skipped,
			Taken = (int)(taken < 0 ? total : taken),
			TotalItemCount = total
		};
	}
}