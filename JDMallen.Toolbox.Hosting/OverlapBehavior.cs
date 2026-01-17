namespace JDMallen.Toolbox.Hosting;

/// <summary>
///   Defines how a background service should handle overlapping executions when work takes
///   longer than the configured <see cref="ScopedBackgroundService{TService}.LoopDelay" />.
/// </summary>
public enum OverlapBehavior
{
	/// <summary>
	///   Allow overlapping executions. New executions will start even if previous ones are
	///   still running. This is the default behavior for backward compatibility.
	/// </summary>
	/// <remarks>
	///   Use this when:
	///   - Executions are independent and can run concurrently
	///   - You have adequate resource capacity
	///   - Order of completion doesn't matter
	/// </remarks>
	AllowOverlap = 0,

	/// <summary>
	///   Skip new executions if a previous execution is still running. The service will wait
	///   for the next scheduled interval before attempting to execute again.
	/// </summary>
	/// <remarks>
	///   Use this when:
	///   - You want to prevent resource exhaustion
	///   - Concurrent executions might interfere with each other
	///   - Missing occasional executions is acceptable
	///   Example: A service that processes a queue - if processing is slow, it's better
	///   to skip an iteration than pile up concurrent executions.
	/// </remarks>
	SkipIfBusy = 1,

	/// <summary>
	///   Wait for the previous execution to complete before starting a new one. This ensures
	///   executions never overlap but may cause drift in the execution schedule.
	/// </summary>
	/// <remarks>
	///   Use this when:
	///   - Executions must run sequentially
	///   - All executions must complete (none should be skipped)
	///   - You can tolerate schedule drift
	///   Example: A service that updates a database where each execution must complete
	///   before the next begins to maintain data consistency.
	/// </remarks>
	WaitForCompletion = 2
}
