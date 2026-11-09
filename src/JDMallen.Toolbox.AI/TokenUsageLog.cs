using System.Text.Json;
using System.Text.Json.Serialization;

namespace JDMallen.Toolbox.AI;

/// <summary>
/// Lifetime token totals across every run of an application, as stored by
/// <see cref="TokenUsageLog" />.
/// </summary>
public sealed class TokenUsageTotals
{
	/// <summary>Runs that made at least one request reporting usage.</summary>
	public int Runs { get; set; }

	/// <summary>Requests that reported usage, across all runs.</summary>
	public long Requests { get; set; }

	/// <summary>Input tokens across all runs.</summary>
	public long InputTokens { get; set; }

	/// <summary>Output tokens across all runs.</summary>
	public long OutputTokens { get; set; }

	/// <summary>When the first run was recorded.</summary>
	public DateTimeOffset? Since { get; set; }

	/// <summary><see cref="InputTokens" /> plus <see cref="OutputTokens" />.</summary>
	[JsonIgnore]
	public long TotalTokens => InputTokens + OutputTokens;

	/// <summary>
	/// The mean tokens per run, each figure rounded to the nearest token (half
	/// away from zero) on its own, or null when no run has been recorded.
	/// </summary>
	public TokenUsageAverages? AveragePerRun() =>
		Runs == 0
			? null
			: new TokenUsageAverages(
				Average(InputTokens),
				Average(OutputTokens),
				Average(TotalTokens));

	private long Average(long tokens) =>
		(long)Math.Round(
			(double)tokens / Runs,
			MidpointRounding.AwayFromZero);
}

/// <summary>Mean tokens per run; see <see cref="TokenUsageTotals.AveragePerRun" />.</summary>
/// <param name="InputTokens">Mean input tokens per run.</param>
/// <param name="OutputTokens">Mean output tokens per run.</param>
/// <param name="TotalTokens">
/// Mean total tokens per run. Rounded on its own, so it can differ by one from
/// <paramref name="InputTokens" /> plus <paramref name="OutputTokens" />.
/// </param>
public sealed record TokenUsageAverages(
	long InputTokens,
	long OutputTokens,
	long TotalTokens);

/// <summary>
/// Keeps an application's lifetime <see cref="TokenUsageTotals" /> in a small
/// JSON file. Safe for several processes of the same application at once:
/// updates serialize on a lock file beside it and are written through a
/// temporary file, so an interrupted write can't truncate the totals.
/// </summary>
public static class TokenUsageLog
{
	private const string FILE_NAME = "usage.json";
	private const int LOCK_ATTEMPTS = 40;

	private static readonly TimeSpan SLockRetryDelay =
		TimeSpan.FromMilliseconds(50);

	/// <summary>
	/// <c>usage.json</c> in the application's directory under the local app
	/// data folder: <c>~/.local/share/&lt;app&gt;/</c> on Linux,
	/// <c>%LOCALAPPDATA%\&lt;app&gt;\</c> on Windows and
	/// <c>~/Library/Application Support/&lt;app&gt;/</c> on macOS. Falls back to
	/// <c>$HOME/.local/share</c>, then the current directory, when the
	/// platform reports no such folder.
	/// </summary>
	/// <param name="applicationName">The directory name, e.g. "shelly".</param>
	public static string DefaultPath(string applicationName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

		string baseDirectory = Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData);
		if (!string.IsNullOrEmpty(baseDirectory))
		{
			return Path.Combine(baseDirectory, applicationName, FILE_NAME);
		}

		string? home = Environment.GetEnvironmentVariable("HOME");
		baseDirectory = string.IsNullOrEmpty(home)
			? "."
			: Path.Combine(home, ".local", "share");

		return Path.Combine(baseDirectory, applicationName, FILE_NAME);
	}

	/// <summary>
	/// Reads the totals at <paramref name="path" />; a missing file yields
	/// empty totals. A file that can't be read or parsed yields null, with the
	/// reason in <paramref name="error" />.
	/// </summary>
	public static TokenUsageTotals? Read(string path, out string? error)
	{
		error = null;
		try
		{
			return ReadUnlocked(path);
		}
		catch (Exception ex) when (ex is IOException
			                           or UnauthorizedAccessException
			                           or JsonException)
		{
			error = ex.Message;

			return null;
		}
	}

	/// <summary>
	/// Adds one run's tally to the totals at <paramref name="path" />,
	/// creating the file and its directory if needed. A run that reported no
	/// usage is not counted, so it doesn't drag the average down. An
	/// unparseable file is left untouched rather than overwritten.
	/// </summary>
	/// <param name="path">The totals file, e.g. from <see cref="DefaultPath" />.</param>
	/// <param name="run">The run's usage.</param>
	/// <param name="now">Recorded as <see cref="TokenUsageTotals.Since" /> on the first run.</param>
	/// <param name="error">Why the update failed, when it did.</param>
	/// <returns>False, with the reason in <paramref name="error" />, on failure.</returns>
	public static bool TryRecord(
		string path,
		TokenUsageTally run,
		DateTimeOffset now,
		out string? error)
	{
		ArgumentNullException.ThrowIfNull(run);

		error = null;
		if (!run.HasUsage)
		{
			return true;
		}

		try
		{
			string directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
			Directory.CreateDirectory(directory);
			using FileStream lockStream = AcquireLock($"{path}.lock");

			TokenUsageTotals totals = ReadUnlocked(path);
			totals.Runs++;
			totals.Requests += run.Requests;
			totals.InputTokens += run.InputTokens;
			totals.OutputTokens += run.OutputTokens;
			totals.Since ??= now;

			Write(path, directory, totals);

			return true;
		}
		catch (Exception ex) when (ex is IOException
			                           or UnauthorizedAccessException
			                           or JsonException)
		{
			error = ex.Message;

			return false;
		}
	}

	private static TokenUsageTotals ReadUnlocked(string path)
	{
		if (!File.Exists(path))
		{
			return new TokenUsageTotals();
		}

		using FileStream stream = File.OpenRead(path);

		return JsonSerializer.Deserialize(
				stream,
				TokenUsageLogJsonContext.Default.TokenUsageTotals)
			?? new TokenUsageTotals();
	}

	private static void Write(
		string path,
		string directory,
		TokenUsageTotals totals)
	{
		string temporaryPath = Path.Combine(
			directory,
			$".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
		try
		{
			using (FileStream stream = File.Create(temporaryPath))
			{
				JsonSerializer.Serialize(
					stream,
					totals,
					TokenUsageLogJsonContext.Default.TokenUsageTotals);
			}

			File.Move(temporaryPath, path, true);
		}
		finally
		{
			File.Delete(temporaryPath);
		}
	}

	// FileShare.None takes an exclusive lock (flock on Unix), held until the
	// stream is disposed. The file's contents are never used.
	private static FileStream AcquireLock(string lockPath)
	{
		for (var attempt = 1;; attempt++)
		{
			try
			{
				return new FileStream(
					lockPath,
					FileMode.OpenOrCreate,
					FileAccess.ReadWrite,
					FileShare.None);
			}
			catch (IOException) when (attempt < LOCK_ATTEMPTS)
			{
				Thread.Sleep(SLockRetryDelay);
			}
		}
	}
}

[JsonSourceGenerationOptions(
	WriteIndented = true,
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(TokenUsageTotals))]
internal sealed partial class TokenUsageLogJsonContext : JsonSerializerContext;
