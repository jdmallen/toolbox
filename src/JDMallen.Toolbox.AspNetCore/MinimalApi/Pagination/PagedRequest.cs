using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Pagination;

/// <summary>
/// Interface for paginated requests.
/// </summary>
public interface IPagedRequest
{
	/// <summary>
	/// The maximum allowed page size to prevent excessive data retrieval.
	/// </summary>
	const int MaxPageSize = 100;

	/// <summary>
	/// Gets the page number (1-based index).
	/// </summary>
	int? Page { get; }

	/// <summary>
	/// Gets the number of items per page.
	/// </summary>
	int? PageSize { get; }
}

/// <summary>
/// Base record for paginated requests.
/// </summary>
public record PagedRequest(int? Page, int? PageSize) : IPagedRequest;

/// <summary>
/// Paginated result container.
/// </summary>
public record PagedList<T>(
	// ReSharper disable once NotAccessedPositionalProperty.Global
	List<T> Items,
	int Page,
	int PageSize,
	int TotalItems)
{
	/// <summary>
	/// Gets a value indicating whether there is a next page available.
	/// </summary>
	public bool HasNextPage => Page * PageSize < TotalItems;

	/// <summary>
	/// Gets a value indicating whether there is a previous page available.
	/// </summary>
	public bool HasPreviousPage => Page > 1;

	/// <summary>
	/// Gets the total number of pages based on the total items and page size.
	/// </summary>
	public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

/// <summary>
/// Extension methods for creating paginated responses from IQueryable.
/// </summary>
public static class PaginationExtensions
{
	/// <summary>
	/// Converts an IQueryable to a paginated list asynchronously.
	/// </summary>
	/// <typeparam name="T">The entity type.</typeparam>
	/// <param name="query">The query to paginate.</param>
	/// <param name="request">The pagination request parameters.</param>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>A paginated list containing the requested page of items.</returns>
	public static async Task<PagedList<T>> ToPagedListAsync<T>(
		this IQueryable<T> query,
		IPagedRequest request,
		CancellationToken cancellationToken = default)
	{
		var page = request.Page ?? 1;
		var pageSize = Math.Min(request.PageSize ?? 10, IPagedRequest.MaxPageSize);

		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(page, 0);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

		var totalItems = await query.CountAsync(cancellationToken);
		var items = await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return new PagedList<T>(items, page, pageSize, totalItems);
	}
}
