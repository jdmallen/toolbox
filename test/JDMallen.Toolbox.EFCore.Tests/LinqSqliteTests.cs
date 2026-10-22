using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using JDMallen.Toolbox.Linq;
using JDMallen.Toolbox.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Proves the <c>JDMallen.Toolbox.Linq</c> helpers translate to SQL on EF
/// Core's SQLite provider, including the case-insensitive search styles that
/// the 3.x <c>Filters.Search</c> could only run client-side.
/// </summary>
public sealed class LinqSqliteTests : IAsyncLifetime
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

	private Task<string[]> SearchAsync(string value, SearchStyle searchStyle) =>
		NamesAsync(_context.Gadgets.Search(gadget => gadget.Name, value, searchStyle));

	[Theory]
	[InlineData("ID", new[] { "Middle" })]
	[InlineData("ar", new[] { "Early" })]
	[InlineData("a", new[] { "Early", "Late" })]
	public async Task Search_Contains_TranslatesAndIgnoresCase(
		string value,
		string[] expected)
		=> Assert.Equal(expected, await SearchAsync(value, SearchStyle.Contains));

	[Theory]
	[InlineData("mid", new[] { "Middle" })]
	[InlineData("LA", new[] { "Late" })]
	[InlineData("ate", new string[0])]
	public async Task Search_StartsWith_TranslatesAndIgnoresCase(
		string value,
		string[] expected)
		=> Assert.Equal(expected, await SearchAsync(value, SearchStyle.StartsWith));

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
	public void DateFilters_EmitASingleComparison()
	{
		string sql = _context.Gadgets.CreatedAfter(T1, true).ToQueryString();

		Assert.DoesNotContain("CASE", sql, StringComparison.Ordinal);
		Assert.Contains("\"g\".\"DateCreated\" >= @", sql, StringComparison.Ordinal);
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

	[Fact]
	public async Task OrderByProperty_TranslatesInBothDirections()
	{
		Assert.Equal(
			["Early", "Late", "Middle"],
			await _context.Gadgets
				.OrderByProperty("Name")
				.Select(gadget => gadget.Name)
				.ToListAsync());
		Assert.Equal(
			["Late", "Middle", "Early"],
			await _context.Gadgets
				.OrderByProperty("DateCreated", false)
				.Select(gadget => gadget.Name)
				.ToListAsync());
	}

	[Fact]
	public void Search_Contains_UpperCasesInSql()
	{
		string sql = _context.Gadgets
			.Search(gadget => gadget.Name, "mid", SearchStyle.Contains)
			.ToQueryString();

		Assert.Contains("upper(\"g\".\"Name\")", sql, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Search_Exact_IsCaseSensitiveUnderBinaryCollation()
	{
		Assert.Equal(["Middle"], await SearchAsync(" Middle ", SearchStyle.Exact));
		Assert.Empty(await SearchAsync("middle", SearchStyle.Exact));
	}
}
