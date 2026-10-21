using JDMallen.Toolbox.Resources;

namespace JDMallen.Toolbox.Tests;

/// <summary>
/// Pins bug B6 in <see cref="ResourceLoader" />: since 3.0.0 its line iterator
/// tests <c>(sr.ReadLine() ?? string.Empty) is { } line</c>, which is never
/// false, so after the last line it yields empty strings forever. Every
/// enumeration to the end (such as <c>ToArray()</c> in
/// <c>CustomPasswordValidator.CheckPassword</c>) never returns. 4.0 fixes the
/// loop and moves the loader into AspNetCore; flip these assertions then.
/// </summary>
public class ResourceLoaderTests
{
	[Fact]
	public void CommonPasswords_ContinuesPastTheLastLine()
	{
		string[] pastEnd = [.. ResourceLoader.CommonPasswords.Skip(7186).Take(3)];

		Assert.Equal(["", "", ""], pastEnd);
	}

	[Fact]
	public void FrequencyTable_ContinuesPastTheLastLine()
	{
		string[] pastEnd = [.. ResourceLoader.FrequencyTable.Skip(729).Take(3)];

		Assert.Equal(["", "", ""], pastEnd);
	}

	[Fact]
	public void Contents_EndAtTheFirstEmptyEntry()
	{
		Assert.Equal(
			7186,
			ResourceLoader.CommonPasswords.TakeWhile(line => line.Length > 0).Count());
		Assert.Equal(
			729,
			ResourceLoader.FrequencyTable.TakeWhile(line => line.Length > 0).Count());
	}
}
