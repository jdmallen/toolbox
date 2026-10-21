using JDMallen.Toolbox.Extensions;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Characterization tests for the string extensions in
/// <see cref="StructExtensions" />, which 4.0 keeps but makes nullable-aware.
/// </summary>
public class StringExtensionsTests
{
	[Theory]
	[InlineData(null, false, true, true)]
	[InlineData("", false, true, true)]
	[InlineData("   ", false, false, true)]
	[InlineData("\t\n", false, false, true)]
	[InlineData("x", true, false, false)]
	[InlineData(" x ", true, false, false)]
	public void MatchTheBclStaticMethods(
		string? value,
		bool hasValue,
		bool isNullOrEmpty,
		bool isNullOrWhiteSpace)
	{
		// The receiver is non-nullable today; null is still accepted at runtime.
		// 4.0 changes the receiver to string? so callers don't need the '!'.
		string receiver = value!;

		Assert.Equal(hasValue, receiver.HasValue());
		Assert.Equal(isNullOrEmpty, receiver.IsNullOrEmpty());
		Assert.Equal(isNullOrWhiteSpace, receiver.IsNullOrWhiteSpace());
	}
}
