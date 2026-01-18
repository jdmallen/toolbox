using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// Base class for background services that execute work on a cron schedule.
/// </summary>
/// <typeparam name="TService">
/// The type of the service implementation. Used for generic logging and scoping.
/// </typeparam>
/// <remarks>
///   <para>
///   This class extends <see cref="ScopedBackgroundService{TService}" /> to
///   provide cron-based
///   scheduling using the NCrontab library. Work executes only when the current
///   time matches
///   or exceeds the next scheduled occurrence according to the cron expression.
///   </para>
///   <para>
///   The <see cref="ScopedBackgroundService{TService}.LoopDelay" /> property
///   controls how
///   frequently the time is checked (not when work executes). A shorter delay
///   provides more
///   precise scheduling but uses more CPU cycles.
///   </para>
///   <para>
///   Adapted from:
///   https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/
///   </para>
/// </remarks>
/// <example>
///   <para>Basic usage with a daily cleanup task:</para>
///   <code>
///   public class DailyCleanupService : ScheduledBackgroundService&lt;DailyCleanupService&gt;
///   {
///       public DailyCleanupService(
///           ILogger&lt;DailyCleanupService&gt; logger,
///           IServiceScopeFactory scopeFactory)
///           : base(logger, scopeFactory, "0 2 * * *") // Run at 2 AM daily
///       {
///       }
/// 
///       protected override TimeSpan LoopDelay =&gt; TimeSpan.FromMinutes(1);
/// 
///       protected override async Task ExecuteInScopeAsync(
///           IServiceScope scope,
///           CancellationToken stoppingToken)
///       {
///           var cleanupService = scope.ServiceProvider.GetRequiredService&lt;ICleanupService&gt;();
///           await cleanupService.PerformDailyCleanupAsync(stoppingToken);
///       }
///   }
///   </code>
///   <para>With second-level precision and overlap prevention:</para>
///   <code>
///   public class HighFrequencyService : ScheduledBackgroundService&lt;HighFrequencyService&gt;
///   {
///       public HighFrequencyService(
///           ILogger&lt;HighFrequencyService&gt; logger,
///           IServiceScopeFactory scopeFactory)
///           : base(logger, scopeFactory, "*/30 * * * * *", includeSeconds: true) // Every 30 seconds
///       {
///       }
/// 
///       protected override TimeSpan LoopDelay =&gt; TimeSpan.FromSeconds(1);
///       protected override OverlapBehavior OverlapBehavior =&gt; OverlapBehavior.SkipIfBusy;
/// 
///       protected override async Task ExecuteInScopeAsync(
///           IServiceScope scope,
///           CancellationToken stoppingToken)
///       {
///           var dataService = scope.ServiceProvider.GetRequiredService&lt;IDataService&gt;();
///           await dataService.ProcessDataAsync(stoppingToken);
///       }
///   }
///   </code>
/// </example>
public abstract class
	ScheduledBackgroundService<TService> : ScopedBackgroundService<TService>
{
	private const string DefaultCronSchedule = "*/1 * * * *";
	private CrontabSchedule _schedule;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="ScheduledBackgroundService{TService}" /> class.
	/// </summary>
	/// <param name="logger">The logger for this service.</param>
	/// <param name="scopeFactory">
	/// The service scope factory used to create scopes for each execution iteration.
	/// </param>
	/// <param name="cronSchedule">
	/// The cron expression defining when the service should execute. If invalid,
	/// defaults to
	/// "*/1 * * * *" (every minute) and logs an error.
	/// </param>
	/// <param name="includeSeconds">
	/// If true, the cron expression includes seconds as the first field (6 fields
	/// total).
	/// If false, uses standard 5-field cron format. Default is false.
	/// </param>
	/// <param name="timeProvider">
	/// Optional time provider for testability. If null, uses
	/// <see cref="SystemTimeProvider.Instance" />.
	/// </param>
	/// <remarks>
	///   <para>
	///   Standard cron format (5 fields): minute hour day month day-of-week
	///   </para>
	///   <para>
	///   Extended format with seconds (6 fields): second minute hour day month
	///   day-of-week
	///   </para>
	///   <para>
	///   Examples:
	///   <list type="bullet">
	///     <item>
	///       <description>"0 * * * *" - Every hour at minute 0</description>
	///     </item>
	///     <item>
	///       <description>"*/15 * * * *" - Every 15 minutes</description>
	///     </item>
	///     <item>
	///       <description>"0 2 * * *" - Daily at 2:00 AM</description>
	///     </item>
	///     <item>
	///       <description>"0 0 * * 0" - Weekly on Sunday at midnight</description>
	///     </item>
	///     <item>
	///       <description>
	///       "*/30 * * * * *" - Every 30 seconds (with includeSeconds:
	///       true)
	///       </description>
	///     </item>
	///   </list>
	///   </para>
	/// </remarks>
	protected ScheduledBackgroundService(
		ILogger<TService> logger,
		IServiceScopeFactory scopeFactory,
		string cronSchedule,
		bool includeSeconds = false,
		ITimeProvider? timeProvider = null)
		: base(logger, scopeFactory, timeProvider)
	{
		if (string.IsNullOrWhiteSpace(cronSchedule))
		{
			throw new ArgumentException(
				"Cron schedule cannot be null or whitespace.",
				nameof(cronSchedule));
		}

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

				return _schedule = CrontabSchedule.Parse(DefaultCronSchedule);
			});

		NextRunTime = _schedule!.GetNextOccurrence(TimeProvider.GetUtcNow());
	}

	/// <summary>
	/// Gets the UTC date and time when the next execution should occur.
	/// </summary>
	/// <remarks>
	/// This property is updated after each execution to reflect the next scheduled
	/// occurrence
	/// according to the cron expression. It can be useful for diagnostics or
	/// logging.
	/// </remarks>
	protected DateTime NextRunTime { get; private set; }

	/// <summary>
	/// Checks if the current time has reached or passed the next scheduled execution
	/// time,
	/// and if so, executes the work and updates the next run time.
	/// </summary>
	/// <param name="stoppingToken">The cancellation token.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	/// This method is called repeatedly according to
	/// <see cref="ScopedBackgroundService{TService}.LoopDelay" />.
	/// Work only executes when the current time matches or exceeds
	/// <see cref="NextRunTime" />.
	/// </remarks>
	protected override async Task ExecuteInScopeAsync(
		CancellationToken stoppingToken)
	{
		var currentTime = TimeProvider.GetUtcNow();

		if (currentTime >= NextRunTime)
		{
			await base.ExecuteInScopeAsync(stoppingToken).ConfigureAwait(false);
			NextRunTime = _schedule.GetNextOccurrence(TimeProvider.GetUtcNow());
		}
	}
}
