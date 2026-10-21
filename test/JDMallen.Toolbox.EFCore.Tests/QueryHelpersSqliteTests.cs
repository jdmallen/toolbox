using JDMallen.Toolbox.EFCore.Extensions;
using JDMallen.Toolbox.EFCore.Filters;
using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using JDMallen.Toolbox.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Runs the query helpers against EF Core's SQLite provider, so each one is
/// proven to translate to SQL (or, where it doesn't, that fact is pinned).
/// </summary>
public sealed class QueryHelpersSqliteTests : IAsyncLifetime
{
	private static readonly DateTimeOffset T0 =
		new(
			2026,
			1,
			1,
			12,
			0,
			0,
			TimeSpan.Zero);

	private static readonly DateTime T1 = T0.UtcDateTime.AddHours(1);
	private static readonly DateTime T2 = T0.UtcDateTime.AddHours(2);

	// An in-memory SQLite database lives only as long as its connection stays
	// open, so the test owns the connection.
	private readonly SqliteConnection _connection = new("DataSource=:memory:");

	private readonly FakeTimeProvider _clock = new(T0);

	private GadgetContext _context = null!;

	public async Task InitializeAsync()
	{
		await _connection.OpenAsync();

		DbContextOptions<GadgetContext> options =
			new DbContextOptionsBuilder<GadgetContext>()
				.UseSqlite(_connection)
				.Options;

		_context = new GadgetContext(options, _clock);
		await _context.Database.EnsureCreatedAsync();

		// The audit interceptor stamps the dates from the fake clock: created at
		// T0, T1, T2, and "Early" modified again at T3.
		Gadget early = await AddAsync("Early");
		_clock.Advance(TimeSpan.FromHours(1));
		await AddAsync("Middle");
		_clock.Advance(TimeSpan.FromHours(1));
		await AddAsync("Late");
		_clock.Advance(TimeSpan.FromHours(1));
		early.Name = "Early";
		_context.Entry(early).Property(gadget => gadget.Name).IsModified = true;
		await _context.SaveChangesAsync();
		_context.ChangeTracker.Clear();
	}

	public async Task DisposeAsync()
	{
		await _context.DisposeAsync();
		await _connection.DisposeAsync();
	}

	private async Task<Gadget> AddAsync(string name)
	{
		var gadget = new Gadget { Name = name };
		_context.Gadgets.Add(gadget);
		await _context.SaveChangesAsync();

		return gadget;
	}

	private static async Task<string[]> NamesAsync(IQueryable<Gadget> query) =>
	[
		.. await query
			.Select(gadget => gadget.Name)
			.OrderBy(name => name)
			.ToListAsync(),
	];

	[Fact]
	public async Task Search_Exact_TranslatesAndIsCaseSensitive()
	{
		Assert.Equal(
			["Middle"],
			await NamesAsync(_context.Gadgets.Search(gadget => gadget.Name, " Middle ")));
		Assert.Empty(
			await NamesAsync(_context.Gadgets.Search(gadget => gadget.Name, "middle")));
	}

	[Theory]
	[InlineData(SearchStyle.Contains)]
	[InlineData(SearchStyle.StartsWith)]
	public async Task Search_ContainsAndStartsWith_DoNotTranslate(
		SearchStyle searchStyle)
	{
		// Pinned as-is: the StringComparison overloads have no SQL translation,
		// so these styles only work client-side today. 4.0 fixes this.
		IQueryable<Gadget> query =
			_context.Gadgets.Search(gadget => gadget.Name, "mid", searchStyle);

		InvalidOperationException exception =
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => NamesAsync(query));
		Assert.Contains("could not be translated", exception.Message,
			StringComparison.Ordinal);
	}

	[Fact]
	public async Task OrderBy_TranslatesInBothDirections()
	{
		Assert.Equal(
			["Early", "Late", "Middle"],
			await _context.Gadgets
				.OrderBy("Name", true)
				.Select(gadget => gadget.Name)
				.ToListAsync());
		Assert.Equal(
			["Late", "Middle", "Early"],
			await _context.Gadgets
				.OrderBy("DateCreated", false)
				.Select(gadget => gadget.Name)
				.ToListAsync());
	}

	[Fact]
	public async Task CreatedFilters_Translate()
	{
		Assert.Equal(["Late"], await NamesAsync(_context.Gadgets.CreatedAfter(T1)));
		Assert.Equal(
			["Late", "Middle"],
			await NamesAsync(_context.Gadgets.CreatedAfter(T1, true)));
		Assert.Equal(["Early"], await NamesAsync(_context.Gadgets.CreatedBefore(T1)));
		Assert.Equal(
			["Early", "Middle"],
			await NamesAsync(_context.Gadgets.CreatedBefore(T1, true)));
	}

	[Fact]
	public async Task ModifiedFilters_Translate()
	{
		Assert.Equal(["Early"], await NamesAsync(_context.Gadgets.ModifiedAfter(T2)));
		Assert.Equal(
			["Early", "Late"],
			await NamesAsync(_context.Gadgets.ModifiedAfter(T2, true)));
		Assert.Equal(["Middle"], await NamesAsync(_context.Gadgets.ModifiedBefore(T2)));
		Assert.Equal(
			["Late", "Middle"],
			await NamesAsync(_context.Gadgets.ModifiedBefore(T2, true)));
	}
}
