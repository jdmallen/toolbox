using JDMallen.Toolbox.Extensions;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Tests for <see cref="StringExtensions" />, which match the static
/// <see cref="string" /> methods and (on .NET Core 3.0+) their nullable
/// annotations.
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
		Assert.Equal(hasValue, value.HasValue());
		Assert.Equal(isNullOrEmpty, value.IsNullOrEmpty());
		Assert.Equal(isNullOrWhiteSpace, value.IsNullOrWhiteSpace());
	}

#if !TOOLBOX_NETSTANDARD2_0
	// Compile-time test: the project treats nullable warnings as errors, so
	// this only builds if each check tells the compiler the value isn't null.
	// The netstandard2.0 build has no NotNullWhen attributes, so it's skipped
	// there.
	[Theory]
	[InlineData("text")]
	public void Checks_InformNullableFlowAnalysis(string? value)
	{
		if (value.HasValue())
		{
			Assert.Equal(4, value.Length);
		}

		if (!value.IsNullOrEmpty())
		{
			Assert.Equal(4, value.Length);
		}

		if (!value.IsNullOrWhiteSpace())
		{
			Assert.Equal(4, value.Length);
		}
	}
#endif
}
