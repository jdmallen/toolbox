namespace JDMallen.Toolbox.AI.Tests;

public sealed class TokenUsageLogTests : IDisposable
{
	private readonly string _directory = Path.Combine(
		Path.GetTempPath(),
		$"toolbox-usage-{Guid.NewGuid():N}");

	private string UsagePath => Path.Combine(_directory, "usage.json");

	public void Dispose()
	{
		if (Directory.Exists(_directory))
		{
			Directory.Delete(_directory, true);
		}
	}

	private static TokenUsageTally Tally(params TokenUsage?[] usages)
	{
		var tally = new TokenUsageTally();
		foreach (TokenUsage? usage in usages)
		{
			tally.Add(usage);
		}

		return tally;
	}

	[Fact]
	public void Tally_AddsReportedUsageAndIgnoresNull()
	{
		TokenUsageTally tally =
			Tally(new TokenUsage(100, 10), null, new TokenUsage(50, 5));

		Assert.Equal(2, tally.Requests);
		Assert.Equal(150, tally.InputTokens);
		Assert.Equal(15, tally.OutputTokens);
		Assert.Equal(165, tally.TotalTokens);
		Assert.True(tally.HasUsage);
		Assert.False(Tally((TokenUsage?)null).HasUsage);
	}

	[Fact]
	public void DefaultPath_EndsInTheApplicationsUsageFile()
	{
		string path = TokenUsageLog.DefaultPath("shelly");

		Assert.Equal("usage.json", Path.GetFileName(path));
		Assert.Equal(
			"shelly",
			Path.GetFileName(Path.GetDirectoryName(path)));
	}

	[Fact]
	public void Read_MissingFile_ReturnsEmptyTotals()
	{
		TokenUsageTotals? totals =
			TokenUsageLog.Read(UsagePath, out string? error);

		Assert.Null(error);
		Assert.NotNull(totals);
		Assert.Equal(0, totals.Runs);
		Assert.Null(totals.AveragePerRun());
	}

	[Fact]
	public void TryRecord_AccumulatesAcrossRuns()
	{
		var firstRun = new DateTimeOffset(
			2026,
			11,
			6,
			9,
			0,
			0,
			TimeSpan.Zero);

		Assert.True(
			TokenUsageLog.TryRecord(
				UsagePath,
				Tally(new TokenUsage(100, 10), new TokenUsage(50, 5)),
				firstRun,
				out _));
		Assert.True(
			TokenUsageLog.TryRecord(
				UsagePath,
				Tally(new TokenUsage(200, 20)),
				firstRun.AddDays(1),
				out _));

		TokenUsageTotals totals = TokenUsageLog.Read(UsagePath, out _)!;
		Assert.Equal(2, totals.Runs);
		Assert.Equal(3, totals.Requests);
		Assert.Equal(350, totals.InputTokens);
		Assert.Equal(35, totals.OutputTokens);
		Assert.Equal(firstRun, totals.Since);
		Assert.Empty(
			Directory.GetFiles(_directory, "*.tmp", SearchOption.AllDirectories));
	}

	[Fact]
	public void TryRecord_RunWithoutUsage_IsNotCounted()
	{
		Assert.True(
			TokenUsageLog.TryRecord(
				UsagePath,
				Tally((TokenUsage?)null),
				DateTimeOffset.Now,
				out _));

		Assert.False(File.Exists(UsagePath));
	}

	[Fact]
	public void TryRecord_UnparseableFile_IsLeftUntouched()
	{
		Directory.CreateDirectory(_directory);
		File.WriteAllText(UsagePath, "{ not json");

		bool recorded = TokenUsageLog.TryRecord(
			UsagePath,
			Tally(new TokenUsage(1, 1)),
			DateTimeOffset.Now,
			out string? error);

		Assert.False(recorded);
		Assert.NotNull(error);
		Assert.Equal("{ not json", File.ReadAllText(UsagePath));
		Assert.Null(TokenUsageLog.Read(UsagePath, out string? readError));
		Assert.NotNull(readError);
	}

	[Fact]
	public void Read_FileWrittenByShelly160_KeepsItsTotals()
	{
		// The layout shelly 1.6.0 wrote before this type moved to the toolbox.
		Directory.CreateDirectory(_directory);
		File.WriteAllText(
			UsagePath,
			"""
			{
			  "runs": 4,
			  "requests": 6,
			  "inputTokens": 10000,
			  "outputTokens": 2345,
			  "since": "2026-11-06T09:00:00+00:00"
			}
			""");

		TokenUsageTotals totals = TokenUsageLog.Read(UsagePath, out _)!;

		Assert.Equal(4, totals.Runs);
		Assert.Equal(6, totals.Requests);
		Assert.Equal(12_345, totals.TotalTokens);
		Assert.Equal(
			new DateTimeOffset(
				2026,
				11,
				6,
				9,
				0,
				0,
				TimeSpan.Zero),
			totals.Since);
	}

	[Fact]
	public void AveragePerRun_RoundsEachFigureHalfAwayFromZero()
	{
		var totals = new TokenUsageTotals
		{
			Runs = 4,
			InputTokens = 10_000,
			OutputTokens = 2_345,
		};

		Assert.Equal(
			new TokenUsageAverages(2_500, 586, 3_086),
			totals.AveragePerRun());
	}
}
