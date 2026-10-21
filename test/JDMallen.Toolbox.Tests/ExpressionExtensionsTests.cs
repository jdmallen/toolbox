using System.Linq.Expressions;
using JDMallen.Toolbox.Extensions;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Characterization tests for <see cref="ExpressionExtensions" />, which the
/// query helpers build on.
/// </summary>
public class ExpressionExtensionsTests
{
	private sealed record Person(string Name);

	[Fact]
	public void Compose_FeedsTheFirstBodyIntoTheSecond()
	{
		Expression<Func<Person, string>> name = person => person.Name;

		Expression<Func<Person, int>> nameLength =
			name.Compose(value => value.Length);

		Assert.Equal(5, nameLength.Compile()(new Person("Alice")));
	}

	[Fact]
	public void Compose_KeepsTheFirstParameterAndInlinesTheSecond()
	{
		Expression<Func<Person, string>> name = person => person.Name;

		Expression<Func<Person, bool>> isBob = name.Compose(value => value == "Bob");

		// A single-parameter lambda with no leftover reference to "value" is what
		// lets query providers translate the composed predicate.
		Assert.Same(name.Parameters[0], isBob.Parameters[0]);
		Assert.Equal("person => (person.Name == \"Bob\")", isBob.ToString());
	}

	[Fact]
	public void Replace_SwapsEveryOccurrenceOfTheNode()
	{
		ParameterExpression from = Expression.Parameter(typeof(int), "from");
		ParameterExpression to = Expression.Parameter(typeof(int), "to");
		Expression body = Expression.Add(from, from);

		Expression replaced = body.Replace(from, to);

		Assert.Equal("(to + to)", replaced.ToString());
	}
}
