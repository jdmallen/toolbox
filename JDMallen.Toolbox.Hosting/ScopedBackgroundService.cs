using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
///   Base class for background services that execute work repeatedly with support for scoped
///   dependency injection and overlap prevention.
/// </summary>
/// <typeparam name="TService">
///   The type of the service implementation. Used for generic logging and scoping.
/// </typeparam>
/// <remarks>
///   <para>
///     This class provides a foundation for long-running background tasks that need to execute
///     repeatedly at regular intervals. It handles the common concerns of scoped service
///     lifetime management, error handling, and execution scheduling.
///   </para>
///   <para>
///     Adapted from: https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/
///   </para>
/// </remarks>
/// <example>
///   <para>Basic usage with a simple data processing service:</para>
///   <code>
///   public class DataProcessorService : ScopedBackgroundService&lt;DataProcessorService&gt;
///   {
///       public DataProcessorService(
///           ILogger&lt;DataProcessorService&gt; logger,
///           IServiceScopeFactory scopeFactory)
///           : base(logger, scopeFactory)
///       {
///       }
///
///       protected override TimeSpan LoopDelay =&gt; TimeSpan.FromMinutes(5);
///
///       protected override async Task ExecuteInScopeAsync(
///           IServiceScope scope,
///           CancellationToken stoppingToken)
///       {
///           var dataService = scope.ServiceProvider.GetRequiredService&lt;IDataService&gt;();
///           await dataService.ProcessPendingDataAsync(stoppingToken);
///       }
///   }
///   </code>
///   <para>With overlap prevention to avoid concurrent executions:</para>
///   <code>
///   public class DatabaseCleanupService : ScopedBackgroundService&lt;DatabaseCleanupService&gt;
///   {
///       public DatabaseCleanupService(
///           ILogger&lt;DatabaseCleanupService&gt; logger,
///           IServiceScopeFactory scopeFactory)
///           : base(logger, scopeFactory)
///       {
///       }
///
///       protected override TimeSpan LoopDelay =&gt; TimeSpan.FromHours(1);
///       protected override OverlapBehavior OverlapBehavior =&gt; OverlapBehavior.SkipIfBusy;
///
///       protected override async Task ExecuteInScopeAsync(
///           IServiceScope scope,
///           CancellationToken stoppingToken)
///       {
///           var dbContext = scope.ServiceProvider.GetRequiredService&lt;AppDbContext&gt;();
///           await dbContext.CleanupOldRecordsAsync(stoppingToken);
///       }
///   }
///   </code>
/// </example>
public abstract class ScopedBackgroundService<TService> : BackgroundService
{
	private readonly ILogger<TService> _logger;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ITimeProvider _timeProvider;
	private int _isExecuting;

	/// <summary>
	///   Initializes a new instance of the <see cref="ScopedBackgroundService{TService}" /> class.
	/// </summary>
	/// <param name="logger">The logger for this service.</param>
	/// <param name="scopeFactory">
	///   The service scope factory used to create scopes for each execution iteration.
	/// </param>
	/// <param name="timeProvider">
	///   Optional time provider for testability. If null, uses <see cref="SystemTimeProvider.Instance" />.
	/// </param>
	protected ScopedBackgroundService(
		ILogger<TService> logger,
		IServiceScopeFactory scopeFactory,
		ITimeProvider timeProvider = null)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		_timeProvider = timeProvider ?? SystemTimeProvider.Instance;
	}

	/// <summary>
	///   Gets the delay between execution iterations.
	/// </summary>
	/// <remarks>
	///   <para>
	///     For direct subtypes of this class, this represents the time to wait between the
	///     completion of one execution and the start of the next.
	///   </para>
	///   <para>
	///     For <see cref="ScheduledBackgroundService{TService}" />, this represents how frequently
	///     the time is checked to determine if the next scheduled occurrence should execute.
	///   </para>
	/// </remarks>
	protected abstract TimeSpan LoopDelay { get; }

	/// <summary>
	///   Gets the behavior for handling overlapping executions when work takes longer than
	///   <see cref="LoopDelay" />.
	/// </summary>
	/// <remarks>
	///   Defaults to <see cref="Hosting.OverlapBehavior.AllowOverlap" /> for backward compatibility.
	///   Override this property to change the overlap behavior.
	/// </remarks>
	protected virtual OverlapBehavior OverlapBehavior => OverlapBehavior.AllowOverlap;

	/// <summary>
	///   Gets the time provider used by this service.
	/// </summary>
	/// <remarks>
	///   Exposed for derived classes that need time operations. Primarily used by
	///   <see cref="ScheduledBackgroundService{TService}" /> for cron scheduling.
	/// </remarks>
	protected ITimeProvider TimeProvider => _timeProvider;

	/// <summary>
	///   This method is called when the <see cref="IHostedService" /> starts and returns a task
	///   that represents the lifetime of the long running operation(s) being performed.
	/// </summary>
	/// <param name="stoppingToken">
	///   Triggered when <see cref="BackgroundService.StopAsync" /> is called.
	/// </param>
	/// <returns>
	///   A <see cref="Task" /> that represents the long running operations.
	/// </returns>
	/// <remarks>
	///   <para>
	///     This implementation creates a loop that repeatedly executes work within a scoped
	///     context, with error handling and logging for each iteration. The loop continues
	///     until the stopping token is cancelled.
	///   </para>
	///   <para>
	///     Each execution is wrapped in error handling to prevent a single failure from
	///     stopping the entire service. Unhandled exceptions are logged but do not propagate.
	///   </para>
	/// </remarks>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		do
		{
			// Determine if we should execute based on overlap behavior
			bool shouldExecute = true;
			bool needsWait = false;

			if (OverlapBehavior != OverlapBehavior.AllowOverlap)
			{
				// Try to set _isExecuting to 1 (true). If it's already 1, the exchange fails.
				int previousValue = Interlocked.CompareExchange(ref _isExecuting, 1, 0);

				if (previousValue == 1)
				{
					// Another execution is in progress
					if (OverlapBehavior == OverlapBehavior.SkipIfBusy)
					{
						shouldExecute = false;
						_logger.LogDebug(
							"Skipping execution iteration because previous iteration is still running");
					}
					else if (OverlapBehavior == OverlapBehavior.WaitForCompletion)
					{
						// We'll need to wait, but first delay and try again
						shouldExecute = false;
						needsWait = true;
					}
				}
			}

			if (shouldExecute)
			{
				try
				{
					var sessionId = Guid.NewGuid();
					using var scope = _logger.BeginScope(sessionId);

					_logger.LogTrace("Begin service execution iteration");
					await ExecuteInScopeAsync(stoppingToken).ConfigureAwait(false);
					_logger.LogTrace("End service execution iteration");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Unhandled exception in service execution iteration");
				}
				finally
				{
					// Release the execution lock if overlap prevention is enabled
					if (OverlapBehavior != OverlapBehavior.AllowOverlap)
					{
						Interlocked.Exchange(ref _isExecuting, 0);
					}
				}
			}

			// For WaitForCompletion, if we're waiting, use a shorter delay before retrying
			var delayTime = needsWait
				? TimeSpan.FromMilliseconds(100)
				: LoopDelay;

			await Task.Delay(delayTime, stoppingToken).ConfigureAwait(false);
		} while (!stoppingToken.IsCancellationRequested);
	}

	/// <summary>
	///   Creates a service scope and executes <see cref="ExecuteInScopeAsync(IServiceScope, CancellationToken)" />
	///   within that scope.
	/// </summary>
	/// <param name="stoppingToken">The cancellation token.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	///   Override this method if you need custom scope creation logic. The default implementation
	///   creates a standard service scope and delegates to the abstract
	///   <see cref="ExecuteInScopeAsync(IServiceScope, CancellationToken)" /> method.
	/// </remarks>
	protected virtual Task ExecuteInScopeAsync(CancellationToken stoppingToken)
	{
		using var scope = _scopeFactory.CreateScope();
		return ExecuteInScopeAsync(scope, stoppingToken);
	}

	/// <summary>
	///   Executes the service logic within a dependency injection scope.
	/// </summary>
	/// <param name="scope">
	///   The service scope containing scoped services for this execution iteration.
	/// </param>
	/// <param name="stoppingToken">
	///   The cancellation token that indicates when the service should stop.
	/// </param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	///   <para>
	///     This is the core method that derived classes must implement to define their
	///     background work. It is called repeatedly according to <see cref="LoopDelay" />
	///     and the configured <see cref="OverlapBehavior" />.
	///   </para>
	///   <para>
	///     The <paramref name="scope" /> parameter provides access to scoped services through
	///     its <see cref="IServiceScope.ServiceProvider" />. This ensures proper lifetime
	///     management for services like Entity Framework DbContext instances.
	///   </para>
	///   <para>
	///     Exceptions thrown from this method are caught and logged by the base class, allowing
	///     the service to continue running. If you need different error handling behavior,
	///     implement it within this method.
	///   </para>
	/// </remarks>
	protected abstract Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken);
}
