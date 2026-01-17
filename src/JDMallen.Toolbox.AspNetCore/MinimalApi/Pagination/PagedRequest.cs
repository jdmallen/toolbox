using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
public record PagedList<T>(List<T> Items, int Page, int PageSize, int TotalItems)
{
	public bool HasNextPage { get; }
	public bool HasPreviousPage { get; }
	public int TotalPages { get; }
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
		throw new System.NotImplementedException();
	}
}
