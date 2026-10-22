using System.Linq.Expressions;
using JDMallen.Toolbox.Implementations;
using JDMallen.Toolbox.Linq;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Tests for <c>JDMallen.Toolbox.Linq</c> over LINQ to Objects. SQL
/// translation is covered by <c>LinqSqliteTests</c> in the EFCore test
/// project.
/// </summary>
public class QueryableExtensionsTests
{
	private static readonly DateTime T0 =
		new(
			2026,
			1,
			1,
			12,
			0,
			0,
			DateTimeKind.Utc);

	private static readonly DateTime T1 = T0.AddHours(1);
	private static readonly DateTime T2 = T0.AddHours(2);
	private static readonly DateTime T3 = T0.AddHours(3);

	private sealed record Address(string City);

	// Private on purpose: the 3.x OrderBy(string, bool) threw for non-public
	// types (B5); OrderByProperty must not.
	private sealed record Person(string Name, int Age, Address Address);

	internal sealed class Widget : EntityModel<int>
	{
		public string Name { get; init; } = string.Empty;
	}

	private static IQueryable<Person> People() =>
		new[]
		{
			new Person("Alice", 30, new Address("Denver")),
			new Person("bob", 25, new Address("Austin")),
			new Person("Alfred", 40, new Address("Boston")),
		}.AsQueryable();

	// Created at T0, T1, T2; "Early" was modified again at T3.
	private static IQueryable<Widget> Widgets() =>
		new[]
		{
			new Widget
			{
				Name = "Early",
				DateCreated = T0,
				DateModified = T3,
			},
			new Widget
			{
				Name = "Middle",
				DateCreated = T1,
				DateModified = T1,
			},
			new Widget
			{
				Name = "Late",
				DateCreated = T2,
				DateModified = T2,
			},
		}.AsQueryable();

	private static string[] Names(IQueryable<Person> query) =>
		[.. query.Select(person => person.Name)];

	private static string[] Names(IQueryable<Widget> query) =>
		[.. query.Select(widget => widget.Name).OrderBy(name => name)];

	// Search

	[Theory]
	[InlineData(SearchStyle.Exact, "Alice", new[] { "Alice" })]
	[InlineData(SearchStyle.Exact, "alice", new string[0])]
	[InlineData(SearchStyle.Exact, "  Alice  ", new[] { "Alice" })]
	[InlineData(SearchStyle.Contains, "LF", new[] { "Alfred" })]
	[InlineData(SearchStyle.Contains, "o", new[] { "bob" })]
	[InlineData(SearchStyle.Contains, " ICE ", new[] { "Alice" })]
	[InlineData(SearchStyle.StartsWith, "al", new[] { "Alice", "Alfred" })]
	[InlineData(SearchStyle.StartsWith, "B", new[] { "bob" })]
	[InlineData(SearchStyle.StartsWith, "ice", new string[0])]
	public void Search_MatchesBySearchStyle(
		SearchStyle searchStyle,
		string searchValue,
		string[] expected)
	{
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

		Assert.Same(
			query,
			query.Search(person => person.Name, searchValue, SearchStyle.Contains));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void OrderByProperty_ReturnsTheQueryUnchangedForBlankPath(string? propertyPath)
	{
		IQueryable<Person> query = People();

		Assert.Same(query, query.OrderByProperty(propertyPath));
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("Nope")]
	[InlineData("Address.Nope")]
	public void PropertyPath_RejectsBlankOrUnknownPaths(string? propertyPath)
	{
		Assert.Throws<ArgumentException>(() => PropertyPath.ToLambda<Person>(propertyPath!));
	}

	// Date filters

	[Fact]
	public void CreatedAfter_IsExclusiveByDefault()
	{
		Assert.Equal(["Late"], Names(Widgets().CreatedAfter(T1)));
		Assert.Equal(["Late", "Middle"], Names(Widgets().CreatedAfter(T1, true)));
	}

	[Fact]
	public void CreatedBefore_IsExclusiveByDefault()
	{
		Assert.Equal(["Early"], Names(Widgets().CreatedBefore(T1)));
		Assert.Equal(["Early", "Middle"], Names(Widgets().CreatedBefore(T1, true)));
	}

	[Fact]
	public void DateFilters_EmitASingleComparison()
	{
		// The inclusive flag is resolved before the expression is built, so the
		// predicate holds no conditional for a provider to translate.
		var predicate =
			((MethodCallExpression)Widgets().CreatedAfter(T1, true).Expression)
			.Arguments[1]
			.ToString();

		Assert.DoesNotContain("IIF", predicate, StringComparison.Ordinal);
		Assert.Contains(">=", predicate, StringComparison.Ordinal);
	}

	[Fact]
	public void ModifiedAfter_IsExclusiveByDefault()
	{
		Assert.Equal(["Early"], Names(Widgets().ModifiedAfter(T2)));
		Assert.Equal(["Early", "Late"], Names(Widgets().ModifiedAfter(T2, true)));
	}

	[Fact]
	public void ModifiedBefore_IsExclusiveByDefault()
	{
		Assert.Equal(["Middle"], Names(Widgets().ModifiedBefore(T2)));
		Assert.Equal(["Late", "Middle"], Names(Widgets().ModifiedBefore(T2, true)));
	}

	[Fact]
	public void OrderByProperty_FollowsDottedPaths()
	{
		Assert.Equal(
			["bob", "Alfred", "Alice"],
			Names(People().OrderByProperty("Address.City")));
	}

	[Fact]
	public void OrderByProperty_ProducesAnOrderedQuery()
	{
		// Built with Expression.Call, so the result is a real OrderBy node that
		// ThenBy can follow.
		IQueryable<Person> sorted = People().OrderByProperty("Name");

		Assert.IsType<IOrderedQueryable<Person>>(sorted, exactMatch: false);
		Assert.Equal(
			nameof(Queryable.OrderBy),
			((MethodCallExpression)sorted.Expression).Method.Name);
	}

	// OrderByProperty

	[Fact]
	public void OrderByProperty_SortsAscendingByDefault()
	{
		Assert.Equal(
			["bob", "Alice", "Alfred"],
			Names(People().OrderByProperty("Age")));
	}

	[Fact]
	public void OrderByProperty_SortsDescending()
	{
		Assert.Equal(
			["Alfred", "Alice", "bob"],
			Names(People().OrderByProperty("Age", false)));
	}

	[Fact]
	public void OrderByProperty_ThrowsForAnUnknownProperty()
	{
		Assert.Throws<ArgumentException>(() => People().OrderByProperty("Nope"));
	}

	// PropertyPath

	[Fact]
	public void PropertyPath_BuildsATypedMemberAccessLambda()
	{
		LambdaExpression lambda = PropertyPath.ToLambda<Person>("Address.City");

		Assert.Equal("x => x.Address.City", lambda.ToString());
		Assert.Equal(typeof(string), lambda.ReturnType);
	}

	[Fact]
	public void PropertyPath_RejectsANullType()
	{
		Assert.Throws<ArgumentNullException>(() => PropertyPath.ToLambda(null!, "Name"));
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
		Assert.Throws<ArgumentOutOfRangeException>(()
			=> People().Search(person => person.Name, "x", (SearchStyle)99));
	}
}
