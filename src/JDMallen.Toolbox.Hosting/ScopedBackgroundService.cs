using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// Base class for background services that execute work repeatedly with support
/// for scoped dependency injection and overlap prevention.
/// </summary>
/// <typeparam name="TService">
/// The type of the service implementation. Used for generic logging and scoping.
/// </typeparam>
/// <remarks>
///   <para>
///   This class provides a foundation for long-running background tasks that need
///   to execute repeatedly at regular intervals. It handles the common concerns of
///   scoped service lifetime management, error handling, and execution scheduling.
///   </para>
///   <para>
///   Adapted from:
///   https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/
///   </para>
/// </remarks>
/// <example>
///   <para>Basic usage with overlap prevention:</para>
///   <code>
///     public class DataProcessorService(
///         ILogger&lt;DataProcessorService&gt; logger,
///         IServiceScopeFactory scopeFactory)
///         : ScopedBackgroundService&lt;DataProcessorService&gt;(logger, scopeFactory)
///     {
///         protected override TimeSpan LoopDelay =&gt; TimeSpan.FromMinutes(5);
///         protected override OverlapBehavior OverlapBehavior =&gt; OverlapBehavior.SkipIfBusy;
/// 
///         protected override async Task ExecuteInScopeAsync(
///             IServiceScope scope,
///             CancellationToken cancellationToken)
///         {
///             var dataService = scope.ServiceProvider.GetRequiredService&lt;IDataService&gt;();
///             await dataService.ProcessPendingDataAsync(cancellationToken);
///         }
///     }
///     </code>
/// </example>
public abstract class ScopedBackgroundService<TService> : BackgroundService
{
	private readonly ILogger<TService> _logger;
	private readonly IServiceScopeFactory _scopeFactory;
	private int _isExecuting;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="ScopedBackgroundService{TService}" /> class.
	/// </summary>
	/// <param name="logger">The logger for this service.</param>
	/// <param name="scopeFactory">
	/// The service scope factory used to create scopes for each execution iteration.
	/// </param>
	/// <param name="timeProvider">
	/// Optional time provider for testability. If null, defaults to the
	/// system clock.
	/// </param>
	protected ScopedBackgroundService(
		ILogger<TService> logger,
		IServiceScopeFactory scopeFactory,
#if NET8_0_OR_GREATER
		TimeProvider? timeProvider = null)
#else
		ITimeProvider? timeProvider = null)
#endif
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_scopeFactory = scopeFactory
			?? throw new ArgumentNullException(nameof(scopeFactory));
#if NET8_0_OR_GREATER
		TimeProvider = timeProvider ?? TimeProvider.System;
#else
		TimeProvider = timeProvider ?? SystemTimeProvider.Instance;
#endif
	}

	/// <summary>
	/// Gets the delay between execution iterations.
	/// </summary>
	/// <remarks>
	///   <para>
	///   For direct subtypes of this class, this is the time to wait between the
	///   completion of one execution and the start of the next.
	///   </para>
	///   <para>
	///   For <see cref="ScheduledBackgroundService{TService}" />, this is how
	///   frequently the time is checked to determine if the next scheduled
	///   occurrence should execute.
	///   </para>
	/// </remarks>
	protected abstract TimeSpan LoopDelay { get; }

	/// <summary>
	/// Gets the behavior for handling overlapping executions when work takes longer
	/// than <see cref="LoopDelay" />. Defaults to
	/// <see cref="Hosting.OverlapBehavior.AllowOverlap" />.
	/// </summary>
	protected virtual OverlapBehavior OverlapBehavior => OverlapBehavior.AllowOverlap;

	/// <summary>
	/// Gets the time provider used by this service.
	/// </summary>
	/// <remarks>
	/// Exposed for derived classes that need time operations. Primarily used by
	/// <see cref="ScheduledBackgroundService{TService}" /> for cron scheduling.
	/// On .NET 8+ this is the built-in <c>System.TimeProvider</c> (so tests can
	/// inject a fake such as <c>FakeTimeProvider</c>); on netstandard2.0 it is a
	/// custom <c>ITimeProvider</c>.
	/// </remarks>
#if NET8_0_OR_GREATER
	protected TimeProvider TimeProvider { get; }
#else
	protected ITimeProvider TimeProvider { get; }
#endif

	/// <summary>
	/// Repeatedly executes work within a scoped context until the stopping token is
	/// cancelled. Each iteration is wrapped in error handling so a single failure
	/// does not stop the service.
	/// </summary>
	/// <param name="stoppingToken">
	/// Triggered when <see cref="BackgroundService.StopAsync" /> is called.
	/// </param>
	/// <returns>
	/// A <see cref="Task" /> that represents the long running operations.
	/// </returns>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		do
		{
			var shouldExecute = true;
			var needsWait = false;

			if (OverlapBehavior != OverlapBehavior.AllowOverlap)
			{
				// Try to claim the execution slot. If it's already claimed, the exchange fails.
				int previousValue = Interlocked.CompareExchange(ref _isExecuting, 1, 0);

				if (previousValue == 1)
				{
					switch (OverlapBehavior)
					{
						// Another execution is in progress.
						case OverlapBehavior.SkipIfBusy:
							shouldExecute = false;
							_logger.LogSkippingBusyIteration();

							break;
						case OverlapBehavior.WaitForCompletion:
							shouldExecute = false;
							needsWait = true;

							break;
						case OverlapBehavior.AllowOverlap:
							break;
						default:
							throw new ArgumentOutOfRangeException(
								$"Unsupported overlap behavior: {OverlapBehavior}");
					}
				}
			}

			if (shouldExecute)
			{
				try
				{
					var sessionId = Guid.NewGuid();
					using IDisposable? scope = _logger.BeginScope(sessionId);

					_logger.LogBeginIteration();
					await ExecuteInScopeAsync(stoppingToken).ConfigureAwait(false);
					_logger.LogEndIteration();
				}

				// A single iteration must never tear down the background loop;
				// log whatever it threw and continue on the next tick.
#pragma warning disable CA1031 // Do not catch general exception types
				catch (Exception ex)
				{
					_logger.LogUnhandledIterationException(ex);
				}
#pragma warning restore CA1031 // Do not catch general exception types
				finally
				{
					if (OverlapBehavior != OverlapBehavior.AllowOverlap)
					{
						Interlocked.Exchange(ref _isExecuting, 0);
					}
				}
			}

			// When waiting for an in-flight iteration, use a short delay before retrying.
			TimeSpan delayTime = needsWait
				? TimeSpan.FromMilliseconds(100)
				: LoopDelay;

			await Task.Delay(delayTime, stoppingToken).ConfigureAwait(false);
		}
		while (!stoppingToken.IsCancellationRequested);
	}

	/// <summary>
	/// Creates a service scope and executes
	/// <see cref="ExecuteInScopeAsync(IServiceScope, CancellationToken)" /> within
	/// it.
	/// </summary>
	/// <param name="stoppingToken">The cancellation token.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	/// Override this method if you need custom scope creation logic, as
	/// <see cref="ScheduledBackgroundService{TService}" /> does for its schedule
	/// check. The default implementation creates a standard service scope and
	/// delegates to the abstract
	/// <see cref="ExecuteInScopeAsync(IServiceScope, CancellationToken)" /> method.
	/// </remarks>
	protected virtual async Task ExecuteInScopeAsync(CancellationToken stoppingToken)
	{
		// Await inside the using so the scope outlives the async work; returning the
		// task directly would dispose the scope while the work is still running.
		using IServiceScope scope = _scopeFactory.CreateScope();

		await ExecuteInScopeAsync(scope, stoppingToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Executes the service logic within a dependency injection scope. Derived
	/// classes implement this to define their background work.
	/// </summary>
	/// <param name="scope">
	/// The service scope containing scoped services for this execution iteration.
	/// This ensures proper lifetime management for services like Entity Framework
	/// DbContext instances.
	/// </param>
	/// <param name="cancellationToken">
	/// The cancellation token that indicates when the service should stop.
	/// </param>
	/// <returns>A task representing the asynchronous operation.</returns>
	/// <remarks>
	/// Exceptions thrown from this method are caught and logged by the base class,
	/// allowing the service to continue running. If you need different error
	/// handling behavior, implement it within this method.
	/// </remarks>
	protected abstract Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken cancellationToken);
}
