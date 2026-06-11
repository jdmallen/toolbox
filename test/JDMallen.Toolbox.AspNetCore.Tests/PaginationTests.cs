using JDMallen.Toolbox.AspNetCore.MinimalApi.Pagination;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies the pagination helpers: the computed navigation properties on
/// <see cref="PagedList{T}"/> and the EF Core-backed
/// <see cref="PaginationExtensions.ToPagedListAsync{T}"/> projection.
/// </summary>
public class PaginationTests
{
	private sealed class Gadget
	{
		public int Id { get; init; }
	}

	private sealed class GadgetContext(DbContextOptions<GadgetContext> options)
		: DbContext(options)
	{
		public DbSet<Gadget> Gadgets => Set<Gadget>();
	}

	// A fresh in-memory store seeded with the requested number of gadgets.
	private static GadgetContext SeededContext(int gadgetCount)
	{
		var options = new DbContextOptionsBuilder<GadgetContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		var context = new GadgetContext(options);
		context.Gadgets.AddRange(
			Enumerable.Range(1, gadgetCount).Select(id => new Gadget { Id = id }));
		context.SaveChanges();

		return context;
	}

	[Fact]
	public void HasNextPage_TrueWhenMoreItemsRemain()
	{
		var page = new PagedList<int>([1, 2], Page: 1, PageSize: 2, TotalItems: 5);

		Assert.True(page.HasNextPage);
		Assert.False(page.HasPreviousPage);
	}

	[Fact]
	public void HasNextPage_FalseOnLastPage()
	{
		var page = new PagedList<int>([5], Page: 3, PageSize: 2, TotalItems: 5);

		Assert.False(page.HasNextPage);
		Assert.True(page.HasPreviousPage);
	}

	[Fact]
	public void TotalPages_RoundsUpForPartialFinalPage()
	{
		var page = new PagedList<int>([], Page: 1, PageSize: 2, TotalItems: 5);

		Assert.Equal(3, page.TotalPages);
	}

	[Fact]
	public async Task ToPagedListAsync_ReturnsRequestedPageSlice()
	{
		await using var context = SeededContext(gadgetCount: 25);

		var page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(Page: 2, PageSize: 10));

		Assert.Equal(2, page.Page);
		Assert.Equal(10, page.PageSize);
		Assert.Equal(25, page.TotalItems);
		Assert.Equal([11, 12, 13, 14, 15, 16, 17, 18, 19, 20], page.Items.Select(g => g.Id));
	}

	[Fact]
	public async Task ToPagedListAsync_DefaultsToFirstPageOfTen()
	{
		await using var context = SeededContext(gadgetCount: 25);

		var page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(Page: null, PageSize: null));

		Assert.Equal(1, page.Page);
		Assert.Equal(10, page.PageSize);
		Assert.Equal(10, page.Items.Count);
	}

	[Fact]
	public async Task ToPagedListAsync_ClampsPageSizeToMaximum()
	{
		await using var context = SeededContext(gadgetCount: 150);

		var page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(Page: 1, PageSize: 500));

		Assert.Equal(IPagedRequest.MaxPageSize, page.PageSize);
		Assert.Equal(IPagedRequest.MaxPageSize, page.Items.Count);
	}
}
