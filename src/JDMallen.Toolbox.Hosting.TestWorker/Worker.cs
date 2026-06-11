namespace JDMallen.Toolbox.Hosting.TestWorker;

/// <summary>
/// Test/sample implementation of
/// <see cref="ScheduledBackgroundService{TService}" />
/// for demonstrating and testing background service execution.
/// </summary>
public class Worker : ScheduledBackgroundService<Worker>
{
	private readonly ILogger<Worker> _logger;

	/// <summary>
	/// Initializes a new instance of the <see cref="Worker" /> class.
	/// </summary>
	/// <param name="logger">The logger for this worker.</param>
	/// <param name="scopeFactory">The service scope factory.</param>
	public Worker(
		ILogger<Worker> logger,
		IServiceScopeFactory scopeFactory) : base(
		logger,
		scopeFactory,
		"I am a bad schedule")
	{
		_logger = logger;
	}

	/// <summary>
	/// Gets the delay between schedule checks (2 seconds).
	/// </summary>
	protected override TimeSpan LoopDelay => TimeSpan.FromSeconds(2);

	/// <summary>
	/// Executes the worker logic, logging the current time.
	/// </summary>
	/// <param name="scope">The dependency injection scope for this execution.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

	// ReSharper disable once AsyncMethodWithoutAwait
	protected override async Task ExecuteInScopeAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		IServiceScope scope,
		CancellationToken cancellationToken)
	{
		_logger.LogWorkerRunning(DateTime.Now);
	}
}
