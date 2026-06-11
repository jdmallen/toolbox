using JDMallen.Toolbox.AspNetCore.MinimalApi.Pagination;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.AspNetCore.Tests;

/// <summary>
/// Verifies the pagination helpers: the computed navigation properties on
/// <see cref="PagedList{T}" /> and the EF Core-backed
/// <see cref="PaginationExtensions.ToPagedListAsync{T}" /> projection.
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
		DbContextOptions<GadgetContext> options = new DbContextOptionsBuilder<GadgetContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		var context = new GadgetContext(options);
		context.Gadgets.AddRange(
			Enumerable.Range(1, gadgetCount).Select(id => new Gadget { Id = id }));
		context.SaveChanges();

		return context;
	}

	[Fact]
	public void HasNextPage_FalseOnLastPage()
	{
		var page = new PagedList<int>(
			[5],
			3,
			2,
			5);

		Assert.False(page.HasNextPage);
		Assert.True(page.HasPreviousPage);
	}

	[Fact]
	public void HasNextPage_TrueWhenMoreItemsRemain()
	{
		var page = new PagedList<int>(
			[1, 2],
			1,
			2,
			5);

		Assert.True(page.HasNextPage);
		Assert.False(page.HasPreviousPage);
	}

	[Fact]
	public async Task ToPagedListAsync_ClampsPageSizeToMaximum()
	{
		await using GadgetContext context = SeededContext(150);

		PagedList<Gadget> page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(1, 500));

		Assert.Equal(IPagedRequest.MaxPageSize, page.PageSize);
		Assert.Equal(IPagedRequest.MaxPageSize, page.Items.Count);
	}

	[Fact]
	public async Task ToPagedListAsync_DefaultsToFirstPageOfTen()
	{
		await using GadgetContext context = SeededContext(25);

		PagedList<Gadget> page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(null, null));

		Assert.Equal(1, page.Page);
		Assert.Equal(10, page.PageSize);
		Assert.Equal(10, page.Items.Count);
	}

	[Fact]
	public async Task ToPagedListAsync_ReturnsRequestedPageSlice()
	{
		await using GadgetContext context = SeededContext(25);

		PagedList<Gadget> page = await context.Gadgets
			.OrderBy(gadget => gadget.Id)
			.ToPagedListAsync(new PagedRequest(2, 10));

		Assert.Equal(2, page.Page);
		Assert.Equal(10, page.PageSize);
		Assert.Equal(25, page.TotalItems);
		Assert.Equal([11, 12, 13, 14, 15, 16, 17, 18, 19, 20], page.Items.Select(g => g.Id));
	}

	[Fact]
	public void TotalPages_RoundsUpForPartialFinalPage()
	{
		var page = new PagedList<int>(
			[],
			1,
			2,
			5);

		Assert.Equal(3, page.TotalPages);
	}
}
