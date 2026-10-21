using JDMallen.Toolbox.EFCore.Extensions;
using JDMallen.Toolbox.EFCore.Filters;
using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Characterization tests for <see cref="Filters.Filters" /> and
/// <see cref="QueryExtensions" /> over LINQ-to-objects. They pin today's
/// behavior before 4.0 moves these helpers to <c>JDMallen.Toolbox.Linq</c>;
/// <see cref="QueryHelpersSqliteTests" /> covers SQL translation.
/// </summary>
public class QueryHelpersInMemoryTests
{
	private static readonly DateTime T0 =
		new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

	private static readonly DateTime T1 = T0.AddHours(1);
	private static readonly DateTime T2 = T0.AddHours(2);
	private static readonly DateTime T3 = T0.AddHours(3);

	// Public on purpose: OrderBy(string, bool) binds through dynamic, which only
	// sees public types (see OrderBy_ThrowsForNonPublicTypes).
	public sealed record Address(string City);

	public sealed record Person(string Name, int Age, Address Address);

	private sealed record PrivatePerson(int Age);

	internal sealed record InternalPerson(int Age);

	private static IQueryable<Person> People() =>
		new[]
		{
			new Person("Alice", 30, new Address("Denver")),
			new Person("bob", 25, new Address("Austin")),
			new Person("Alfred", 40, new Address("Boston")),
		}.AsQueryable();

	// Same shape as the SQLite seed: created at T0, T1, T2; "Early" was
	// modified again at T3.
	private static IQueryable<Gadget> Gadgets() =>
		new[]
		{
			new Gadget
			{
				Name = "Early",
				DateCreated = T0,
				DateModified = T3,
			},
			new Gadget
			{
				Name = "Middle",
				DateCreated = T1,
				DateModified = T1,
			},
			new Gadget
			{
				Name = "Late",
				DateCreated = T2,
				DateModified = T2,
			},
		}.AsQueryable();

	private static string[] Names(IQueryable<Person> query) =>
		[.. query.Select(person => person.Name)];

	private static string[] Names(IQueryable<Gadget> query) =>
		[.. query.Select(gadget => gadget.Name).OrderBy(name => name)];

	// Search

	[Theory]
	[InlineData(SearchStyle.Exact, "Alice", new[] { "Alice" })]
	[InlineData(SearchStyle.Exact, "alice", new string[0])]
	[InlineData(SearchStyle.Exact, "  Alice  ", new[] { "Alice" })]
	[InlineData(SearchStyle.Contains, "LF", new[] { "Alfred" })]
	[InlineData(SearchStyle.Contains, "o", new[] { "bob" })]
	[InlineData(SearchStyle.StartsWith, "al", new[] { "Alice", "Alfred" })]
	[InlineData(SearchStyle.StartsWith, "B", new[] { "bob" })]
	[InlineData(SearchStyle.StartsWith, "ice", new string[0])]
	public void Search_MatchesBySearchStyle(
		SearchStyle searchStyle,
		string searchValue,
		string[] expected)
	{
		// Exact is case-sensitive; Contains and StartsWith ignore case. All three
		// trim the search value first.
		IQueryable<Person> result =
			People().Search(person => person.Name, searchValue, searchStyle);

		Assert.Equal(expected, Names(result));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Search_ReturnsTheQueryUnchangedForBlankInput(string? searchValue)
	{
		IQueryable<Person> query = People();

		IQueryable<Person> result =
			query.Search(person => person.Name, searchValue!, SearchStyle.Exact);

		Assert.Same(query, result);
	}

	[Fact]
	public void Search_DefaultsToExact()
	{
		IQueryable<Person> result = People().Search(person => person.Name, "bob");

		Assert.Equal(["bob"], Names(result));
	}

	[Fact]
	public void Search_ThrowsForAnUndefinedSearchStyle()
	{
		Assert.Throws<ArgumentOutOfRangeException>(
			() => People().Search(person => person.Name, "x", (SearchStyle)99));
	}

	// OrderBy(string, bool)

	[Fact]
	public void OrderBy_SortsByNamedPropertyAscending()
	{
		IQueryable<Person> result = People().OrderBy("Age", true);

		Assert.Equal(["bob", "Alice", "Alfred"], Names(result));
	}

	[Fact]
	public void OrderBy_SortsByNamedPropertyDescending()
	{
		IQueryable<Person> result = People().OrderBy("Age", false);

		Assert.Equal(["Alfred", "Alice", "bob"], Names(result));
	}

	[Fact]
	public void OrderBy_FollowsDottedPaths()
	{
		IQueryable<Person> result = People().OrderBy("Address.City", true);

		Assert.Equal(["bob", "Alfred", "Alice"], Names(result));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void OrderBy_ReturnsTheQueryUnchangedForBlankName(string? fieldName)
	{
		IQueryable<Person> query = People();

		Assert.Same(query, query.OrderBy(fieldName!, true));
	}

	[Fact]
	public void OrderBy_ThrowsForAnUnknownProperty()
	{
		Assert.Throws<ArgumentException>(
			() => People().OrderBy("Nope", true));
	}

	[Fact]
	public void OrderBy_ThrowsForNonPublicTypes()
	{
		// Pinned bug (B5): the dynamic call to Queryable.OrderBy is bound from
		// inside the EFCore assembly, where a private or internal entity type is
		// inaccessible, so the binder falls back to object and finds no
		// overload. 4.0's OrderByProperty builds the call with Expression.Call
		// instead.
		IQueryable<PrivatePerson> privatePeople =
			new[] { new PrivatePerson(1) }.AsQueryable();
		IQueryable<InternalPerson> internalPeople =
			new[] { new InternalPerson(1) }.AsQueryable();

		Assert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(
			() => privatePeople.OrderBy("Age", true));
		Assert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(
			() => internalPeople.OrderBy("Age", true));
	}

	// Date filters

	[Fact]
	public void CreatedAfter_IsExclusiveByDefault()
	{
		Assert.Equal(["Late"], Names(Gadgets().CreatedAfter(T1)));
		Assert.Equal(["Late", "Middle"], Names(Gadgets().CreatedAfter(T1, true)));
	}

	[Fact]
	public void CreatedBefore_IsExclusiveByDefault()
	{
		Assert.Equal(["Early"], Names(Gadgets().CreatedBefore(T1)));
		Assert.Equal(
			["Early", "Middle"],
			Names(Gadgets().CreatedBefore(T1, true)));
	}

	[Fact]
	public void ModifiedAfter_IsExclusiveByDefault()
	{
		Assert.Equal(["Early"], Names(Gadgets().ModifiedAfter(T2)));
		Assert.Equal(["Early", "Late"], Names(Gadgets().ModifiedAfter(T2, true)));
	}

	[Fact]
	public void ModifiedBefore_IsExclusiveByDefault()
	{
		Assert.Equal(["Middle"], Names(Gadgets().ModifiedBefore(T2)));
		Assert.Equal(
			["Late", "Middle"],
			Names(Gadgets().ModifiedBefore(T2, true)));
	}
}
