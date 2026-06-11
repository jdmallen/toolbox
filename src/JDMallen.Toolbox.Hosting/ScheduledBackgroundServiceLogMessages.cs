using Microsoft.Extensions.Logging;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// High-performance logging messages for
/// <see cref="ScheduledBackgroundService{TService}" /> using LoggerMessage source
/// generators.
/// </summary>
internal static partial class ScheduledBackgroundServiceLogMessages
{
	[LoggerMessage(
		EventId = 404,
		Level = LogLevel.Error,
		Message = "Unable to parse cron schedule; defaulting to \"{DefaultCronSchedule}\"")]
	public static partial void LogUnableToParseCronSchedule(
		this ILogger logger,
		Exception exception,
		string defaultCronSchedule);
}
