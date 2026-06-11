namespace JDMallen.Toolbox.Hosting.TestWorker;

/// <summary>
/// High-performance logging messages for <see cref="Worker" /> using LoggerMessage
/// source generators.
/// </summary>
internal static partial class WorkerLogMessages
{
	[LoggerMessage(
		EventId = 500,
		Level = LogLevel.Information,
		Message = "Worker running at: {Time}")]
	public static partial void LogWorkerRunning(this ILogger logger, DateTime time);
}
