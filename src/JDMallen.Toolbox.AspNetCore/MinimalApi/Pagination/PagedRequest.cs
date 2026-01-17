using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.AspNetCore.MinimalApi.Pagination;

/// <summary>
/// Interface for paginated requests.
/// </summary>
public interface IPagedRequest
{
	const int MaxPageSize = 100;
	int? Page { get; }
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
	List<T> Items,
	int Page,
	int PageSize,
	int TotalItems)
{
	public bool HasNextPage => Page * PageSize < TotalItems;
	public bool HasPreviousPage => Page > 1;
	public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}

/// <summary>
/// Extension methods for creating paginated responses from IQueryable.
/// </summary>
public static class PaginationExtensions
{
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
