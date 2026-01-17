using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
///   Adapted from https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/
/// </summary>
/// <typeparam name="TService"></typeparam>
public abstract class ScheduledBackgroundService<TService> : ScopedBackgroundService<TService>
{
	private const string DefaultCronSchedule = "*/1 * * * *";
	private CrontabSchedule _schedule;

	protected ScheduledBackgroundService(
		ILogger<TService> logger,
		IServiceScopeFactory scopeFactory,
		string cronSchedule,
		bool includeSeconds = false)
		: base(logger, scopeFactory)
	{
		// Try to parse passed schedule.
		// If it fails, default to run once per minute and log the failure.
		var options = new CrontabSchedule.ParseOptions
		{
			IncludingSeconds = includeSeconds
		};

		CrontabSchedule.TryParse(
			cronSchedule,
			options,
			schedule => _schedule = schedule,
			provider =>
			{
				logger.LogError(
					provider.Invoke(),
					"Unable to parse cron schedule; defaulting to \"{DefaultCronSchedule}\"",
					DefaultCronSchedule);

				return _schedule = CrontabSchedule.Parse(
					DefaultCronSchedule);
			});
		NextRunTime = _schedule.GetNextOccurrence(DateTime.UtcNow);
	}

	// ReSharper disable once MemberCanBePrivate.Global
	protected DateTime NextRunTime { get; private set; }

	protected override async Task ExecuteInScopeAsync(CancellationToken stoppingToken)
	{
		if (DateTime.UtcNow >= NextRunTime)
		{
			await base.ExecuteInScopeAsync(stoppingToken);
			NextRunTime = _schedule.GetNextOccurrence(DateTime.UtcNow);
		}
	}
}