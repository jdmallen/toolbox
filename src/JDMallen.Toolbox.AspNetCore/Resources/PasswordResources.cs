using System.Globalization;
using System.Text;

namespace JDMallen.Toolbox.AspNetCore.Resources;

/// <summary>
/// The embedded data behind <c>CustomPasswordValidator</c>, loaded and parsed
/// once on first use.
/// </summary>
internal static class PasswordResources
{
	/// <summary>
	/// The size of the frequency table's alphabet: index 0 for anything that
	/// isn't a letter, then 'a' to 'z' as 1 to 26.
	/// </summary>
	internal const int AlphabetSize = 27;

	/// <summary>
	/// Common passwords, lowercase, for exact (case-folded) matching.
	/// </summary>
	internal static HashSet<string> CommonPasswords { get; } =
		new(ReadLines("CommonPasswords.txt"), StringComparer.Ordinal);

	/// <summary>
	/// Letter-pair frequencies, row-major: the entry for a pair is at
	/// <c>first * AlphabetSize + second</c>.
	/// </summary>
	internal static float[] FrequencyTable { get; } = ParseFrequencyTable();

	// The table is stored as text with '.' decimals, so it must be parsed with
	// the invariant culture. Parsing with the current culture reads 0.2365 as
	// 2365 (thousands separator) under de-DE and fails outright under fr-FR,
	// which made every password score as near-maximum strength.
	internal static float[] ParseFrequencyTable()
	{
		float[] table =
		[
			.. ReadLines("FrequencyTable.txt")
				.Select(line => float.Parse(
					line,
					NumberStyles.Float,
					CultureInfo.InvariantCulture)),
		];

		if (table.Length != AlphabetSize * AlphabetSize)
		{
			throw new InvalidOperationException(
				$"FrequencyTable.txt has {table.Length} entries; expected "
				+ $"{AlphabetSize * AlphabetSize}.");
		}

		return table;
	}

	private static List<string> ReadLines(string fileName)
	{
		using Stream stream = typeof(PasswordResources).Assembly
				.GetManifestResourceStream(typeof(PasswordResources), fileName)
			?? throw new InvalidOperationException(
				$"Embedded resource {fileName} is missing.");
		using var reader = new StreamReader(stream, Encoding.UTF8);

		var lines = new List<string>();
		while (reader.ReadLine() is { } line)
		{
			lines.Add(line);
		}

		return lines;
	}
}
