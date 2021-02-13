using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace JDMallen.Toolbox.Hosting
{
	/// <summary>
	/// Adapted from https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/
	/// </summary>
	/// <typeparam name="TService"></typeparam>
	public abstract class ScheduledBackgroundService<TService> : ScopedBackgroundService<TService>
	{
		private CrontabSchedule _schedule;

		protected DateTime NextRunTime { get; private set; }

		private const string DefaultCronSchedule = "*/1 * * * *";

		protected ScheduledBackgroundService(
			ILogger<TService> logger,
			IServiceScopeFactory scopeFactory,
			string cronSchedule,
			bool includeSeconds = true)
			: base(logger, scopeFactory)
		{
			// Try to parse passed schedule.
			// If it fails, default to run once per minute and log the failure.
			CrontabSchedule.TryParse(
				cronSchedule,
				null,
				schedule => _schedule = schedule,
				provider =>
				{
					logger.LogError(
						$"Unable to parse cron schedule; defaulting to \"{DefaultCronSchedule}\".",
						provider.Invoke());

					return _schedule = CrontabSchedule.Parse(
						DefaultCronSchedule);
				});
			NextRunTime = _schedule.GetNextOccurrence(DateTime.Now);
			IEnumerable<DateTime> nextOccurrences = _schedule.GetNextOccurrences(
				DateTime.Now,
				DateTime.Now.AddHours(1));
			foreach (DateTime nextOccurrence in nextOccurrences)
			{
				Console.WriteLine("{0:O}", nextOccurrence);
			}
		}

		protected override async Task ExecuteInScopeAsync(CancellationToken stoppingToken)
		{
			if (DateTime.Now >= NextRunTime)
			{
				await base.ExecuteInScopeAsync(stoppingToken);
				NextRunTime = _schedule.GetNextOccurrence(DateTime.Now);
			}
		}
	}
}
