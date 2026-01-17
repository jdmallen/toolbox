# JDMallen.Toolbox.Hosting

Background service base classes with support for scoped services and cron-based
scheduling.

## Overview

This library provides two abstract base classes that simplify the creation of
background services in .NET applications:

- **`ScopedBackgroundService<TService>`** - Execute work repeatedly at fixed
  intervals with automatic dependency injection scope management
- **`ScheduledBackgroundService<TService>`** - Execute work on a cron schedule
  using the NCrontab library

Both classes are built on top of
`Microsoft.Extensions.Hosting.BackgroundService` and handle the complexity of
scoped service lifetimes, error handling, and execution scheduling.

## Installation

```bash
dotnet add package JDMallen.Toolbox.Hosting
```

## Key Features

- **Automatic Scope Management** - Each execution runs in its own DI scope,
  perfect for Entity Framework DbContext and other scoped services
- **Overlap Prevention** - Configure how to handle executions that take longer
  than the scheduled interval (skip, wait, or allow)
- **Testable Time Operations** - `ITimeProvider` abstraction makes time-based
  logic unit testable
- **Robust Error Handling** - Unhandled exceptions are logged but don't crash
  the service
- **Cron Scheduling** - Full cron expression support with optional second-level
  precision
- **Comprehensive Logging** - Built-in trace logging with session IDs for
  correlation

## Quick Start

### Simple Periodic Service

Create a service that executes every 5 minutes:

```csharp
public class DataProcessorService : ScopedBackgroundService<DataProcessorService>
{
	public DataProcessorService(
		ILogger<DataProcessorService> logger,
		IServiceScopeFactory scopeFactory)
		: base(logger, scopeFactory)
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromMinutes(5);

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
		await dataService.ProcessPendingDataAsync(stoppingToken);
	}
}
```

Register it in your host builder:

```csharp
builder.Services.AddHostedService<DataProcessorService>();
```

### Cron-Scheduled Service

Create a service that runs daily at 2 AM:

```csharp
public class DailyCleanupService : ScheduledBackgroundService<DailyCleanupService>
{
	public DailyCleanupService(
		ILogger<DailyCleanupService> logger,
		IServiceScopeFactory scopeFactory)
		: base(logger, scopeFactory, "0 2 * * *") // 2 AM daily
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromMinutes(1);

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		var cleanupService = scope.ServiceProvider.GetRequiredService<ICleanupService>();
		await cleanupService.PerformDailyCleanupAsync(stoppingToken);
	}
}
```

## Common Usage Patterns

### Preventing Overlapping Executions

Use `OverlapBehavior` to control what happens when work takes longer than the
`LoopDelay`:

```csharp
public class DatabaseCleanupService : ScopedBackgroundService<DatabaseCleanupService>
{
	public DatabaseCleanupService(
		ILogger<DatabaseCleanupService> logger,
		IServiceScopeFactory scopeFactory)
		: base(logger, scopeFactory)
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromHours(1);

	// Skip execution if previous one is still running
	protected override OverlapBehavior OverlapBehavior => OverlapBehavior.SkipIfBusy;

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		await dbContext.CleanupOldRecordsAsync(stoppingToken);
	}
}
```

#### Overlap Behavior Options

- **`AllowOverlap`** (default) - New executions start even if previous ones are
  still running
  - Use when: Executions are independent and can run concurrently
- **`SkipIfBusy`** - Skip new executions if a previous execution is still
  running
  - Use when: You want to prevent resource exhaustion or concurrent executions
    might interfere
- **`WaitForCompletion`** - Wait for previous execution to complete before
  starting next
  - Use when: Executions must run sequentially and none should be skipped

### High-Frequency Cron Scheduling

Use second-level precision for frequent executions:

```csharp
public class HighFrequencyService : ScheduledBackgroundService<HighFrequencyService>
{
	public HighFrequencyService(
		ILogger<HighFrequencyService> logger,
		IServiceScopeFactory scopeFactory)
		: base(logger, scopeFactory, "*/30 * * * * *", includeSeconds: true)
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromSeconds(1);
	protected override OverlapBehavior OverlapBehavior => OverlapBehavior.SkipIfBusy;

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();
		await dataService.ProcessDataAsync(stoppingToken);
	}
}
```

### Using Configuration for Schedules

Read cron schedules from configuration:

```csharp
public class ConfigurableService : ScheduledBackgroundService<ConfigurableService>
{
	public ConfigurableService(
		ILogger<ConfigurableService> logger,
		IServiceScopeFactory scopeFactory,
		IConfiguration configuration)
		: base(
			logger,
			scopeFactory,
			configuration["BackgroundServices:MyService:CronSchedule"] ?? "0 * * * *")
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromMinutes(1);

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		// Implementation
	}
}
```

In `appsettings.json`:

```json
{
	"BackgroundServices": {
		"MyService": {
			"CronSchedule": "0 2 * * *"
		}
	}
}
```

### Testable Services with ITimeProvider

For unit testing, inject a mock time provider:

```csharp
public class TestableService : ScheduledBackgroundService<TestableService>
{
	public TestableService(
		ILogger<TestableService> logger,
		IServiceScopeFactory scopeFactory,
		string cronSchedule,
		ITimeProvider timeProvider = null)
		: base(logger, scopeFactory, cronSchedule, false, timeProvider)
	{
	}

	protected override TimeSpan LoopDelay => TimeSpan.FromMinutes(1);

	protected override async Task ExecuteInScopeAsync(
		IServiceScope scope,
		CancellationToken stoppingToken)
	{
		// Implementation
	}
}
```

In your tests:

```csharp
public class MockTimeProvider : ITimeProvider
{
	public DateTime CurrentTime { get; set; }
	public DateTime GetUtcNow() => CurrentTime;
}

[Fact]
public async Task Service_ExecutesAtScheduledTime()
{
	var mockTime = new MockTimeProvider { CurrentTime = new DateTime(2025, 1, 1, 12, 0, 0) };
	var service = new TestableService(logger, scopeFactory, "0 12 * * *", mockTime);

	// Test service behavior with controlled time
}
```

## Cron Expression Reference

### Standard Format (5 fields)

```
┌─────────── minute (0 - 59)
│ ┌───────── hour (0 - 23)
│ │ ┌─────── day of month (1 - 31)
│ │ │ ┌───── month (1 - 12)
│ │ │ │ ┌─── day of week (0 - 6) (Sunday to Saturday)
│ │ │ │ │
* * * * *
```

### Extended Format with Seconds (6 fields)

```
┌───────────── second (0 - 59)
│ ┌─────────── minute (0 - 59)
│ │ ┌───────── hour (0 - 23)
│ │ │ ┌─────── day of month (1 - 31)
│ │ │ │ ┌───── month (1 - 12)
│ │ │ │ │ ┌─── day of week (0 - 6)
│ │ │ │ │ │
* * * * * *
```

### Common Examples

| Schedule             | Cron Expression  | Description                                             |
|----------------------|------------------|---------------------------------------------------------|
| Every minute         | `* * * * *`      | Runs every minute                                       |
| Every 5 minutes      | `*/5 * * * *`    | Runs every 5 minutes                                    |
| Every hour           | `0 * * * *`      | Runs at minute 0 of every hour                          |
| Every day at 2 AM    | `0 2 * * *`      | Runs daily at 2:00 AM                                   |
| Every Monday at 9 AM | `0 9 * * 1`      | Runs every Monday at 9:00 AM                            |
| Every 30 seconds     | `*/30 * * * * *` | Runs every 30 seconds (requires `includeSeconds: true`) |
| First day of month   | `0 0 1 * *`      | Runs at midnight on the 1st of each month               |
| Weekdays at 8 AM     | `0 8 * * 1-5`    | Runs Monday-Friday at 8:00 AM                           |

## Best Practices

### 1. Choose Appropriate LoopDelay

For `ScopedBackgroundService`:

- The delay represents wait time **between** executions
- Shorter delays = more responsive but higher CPU usage
- Match to your use case (seconds for high-frequency, minutes for periodic
  tasks)

For `ScheduledBackgroundService`:

- The delay controls schedule checking frequency, not execution frequency
- Too short = unnecessary CPU cycles
- Too long = imprecise scheduling
- Recommendation: 1 minute for most cases, 1 second if you need second-level
  precision

### 2. Use Overlap Prevention Wisely

```csharp
// Good: Prevent resource exhaustion
protected override OverlapBehavior OverlapBehavior => OverlapBehavior.SkipIfBusy;

// Good: Ensure sequential database migrations
protected override OverlapBehavior OverlapBehavior => OverlapBehavior.WaitForCompletion;

// Careful: Can cause service collapse under heavy load
protected override OverlapBehavior OverlapBehavior => OverlapBehavior.AllowOverlap;
```

### 3. Handle Cancellation Properly

Always respect the `CancellationToken`:

```csharp
protected override async Task ExecuteInScopeAsync(
	IServiceScope scope,
	CancellationToken stoppingToken)
{
	var items = await GetItemsAsync(stoppingToken);

	foreach (var item in items)
	{
		// Check token periodically in loops
		stoppingToken.ThrowIfCancellationRequested();

		await ProcessItemAsync(item, stoppingToken);
	}
}
```

### 4. Use Scoped Services Correctly

Access services through the scope's `ServiceProvider`:

```csharp
protected override async Task ExecuteInScopeAsync(
	IServiceScope scope,
	CancellationToken stoppingToken)
{
	// Good: Get service from scope
	var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	// Bad: Don't inject scoped services in constructor
	// (they'll be created once and reused, breaking the scoped lifetime)
}
```

### 5. Log Appropriately

The base class provides trace logging for execution start/end. Add your own
logging for important events:

```csharp
protected override async Task ExecuteInScopeAsync(
	IServiceScope scope,
	CancellationToken stoppingToken)
{
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<MyService>>();

	var items = await GetItemsAsync(stoppingToken);
	logger.LogInformation("Processing {Count} items", items.Count);

	// Process items...

	logger.LogInformation("Processed {Count} items successfully", items.Count);
}
```

### 6. Handle Invalid Cron Schedules

The library automatically falls back to `*/1 * * * *` (every minute) if the cron
expression is invalid, but you should validate schedules at configuration time:

```csharp
// In Startup.cs or Program.cs
var cronSchedule = configuration["CronSchedule"] ?? "0 * * * *";

try
{
	var schedule = NCrontab.CrontabSchedule.Parse(cronSchedule);
	builder.Services.AddHostedService(sp => new MyService(
		sp.GetRequiredService<ILogger<MyService>>(),
		sp.GetRequiredService<IServiceScopeFactory>(),
		cronSchedule));
}
catch (Exception ex)
{
	throw new InvalidOperationException($"Invalid cron schedule: {cronSchedule}", ex);
}
```

## Architecture Notes

### Why Scoped Services?

Many .NET services (like Entity Framework's `DbContext`) are registered as
scoped, meaning they're designed to live for the duration of a single request or
operation. Background services are singletons that run for the entire
application lifetime, so they can't directly use scoped services.

These base classes solve this by creating a new DI scope for each execution,
allowing you to safely use scoped services in your background work.

### Thread Safety

The overlap prevention mechanism uses `Interlocked.CompareExchange` for
lock-free thread safety. This ensures efficient execution tracking without the
overhead of locks.

### Performance Considerations

- `ConfigureAwait(false)` is used throughout to avoid unnecessary
  synchronization context captures
- The execution loop is optimized to minimize allocations
- Scope creation is the primary overhead - match `LoopDelay` to your performance
  requirements

## Dependencies

- **Microsoft.Extensions.Hosting.Abstractions** (v10.0.2) - For
  `BackgroundService` and DI abstractions
- **NCrontab** (v3.4.0) - For cron expression parsing (only needed if using
  `ScheduledBackgroundService`)

## Target Framework

- **netstandard2.1** - Compatible with .NET Core 3.0+, .NET 5+, and .NET
  Framework 4.8+

## License

See the repository root for license information.

## Contributing

Contributions are welcome! Please follow the existing code style and include
tests for new features.

## Related Resources

- [Original blog post: ASP.NET Core Background Processing](https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/)
- [Original blog post: Scheduled Background Tasks](https://thinkrethink.net/2018/05/31/run-scheduled-background-tasks-in-asp-net-core/)
- [NCrontab documentation](https://github.com/atifaziz/NCrontab)
- [Microsoft: Background tasks with hosted services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
