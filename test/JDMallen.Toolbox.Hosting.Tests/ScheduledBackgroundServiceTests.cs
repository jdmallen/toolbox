using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.Hosting.Tests;

/// <summary>
/// Verifies that <see cref="ScheduledBackgroundService{TService}" /> reads the
/// clock through the injected <see cref="TimeProvider" />, so a
/// <see cref="FakeTimeProvider" /> fully controls when scheduled work fires. This
/// is the interop payoff of swapping the custom netstandard time abstraction for
/// the built-in <see cref="TimeProvider" /> on .NET 8+.
/// </summary>
public class ScheduledBackgroundServiceTests
{
	// A clean minute boundary keeps the expected cron occurrences obvious.
	private static readonly DateTimeOffset StartInstant =
		new(2026, 6, 10, 8, 0, 0, TimeSpan.Zero);

	/// <summary>
	/// Concrete service scheduled to run every minute. It counts executions and
	/// exposes the otherwise-protected per-iteration schedule check so tests can
	/// drive scheduling decisions deterministically instead of racing the loop.
	/// </summary>
	private sealed class CountingScheduledService(
		IServiceScopeFactory scopeFactory,
		TimeProvider timeProvider)
		: ScheduledBackgroundService<CountingScheduledService>(
			NullLogger<CountingScheduledService>.Instance,
			scopeFactory,
			"* * * * *",
			includeSeconds: false,
			timeProvider)
	{
		private int _executionCount;

		public int ExecutionCount => Volatile.Read(ref _executionCount);

		// Fast loop so the hosted-lifecycle test reacts promptly to clock advances.
		protected override TimeSpan LoopDelay => TimeSpan.FromMilliseconds(20);

		protected override Task ExecuteInScopeAsync(
			IServiceScope scope,
			CancellationToken stoppingToken)
		{
			Interlocked.Increment(ref _executionCount);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Runs a single schedule-check iteration (the work fires only if the
		/// clock has reached the next occurrence).
		/// </summary>
		public Task RunScheduleCheckAsync(CancellationToken stoppingToken = default) =>
			ExecuteInScopeAsync(stoppingToken);
	}

	private static IServiceScopeFactory CreateScopeFactory() =>
		new ServiceCollection()
			.BuildServiceProvider()
			.GetRequiredService<IServiceScopeFactory>();

	[Fact]
	public async Task DoesNotExecuteBeforeScheduledTimeIsReached()
	{
		var fakeTimeProvider = new FakeTimeProvider(StartInstant);
		var service = new CountingScheduledService(
			CreateScopeFactory(),
			fakeTimeProvider);

		// Next occurrence is 08:01:00; advancing only 30s stays short of it.
		fakeTimeProvider.Advance(TimeSpan.FromSeconds(30));
		await service.RunScheduleCheckAsync();

		Assert.Equal(0, service.ExecutionCount);
	}

	[Fact]
	public async Task ExecutesOnceTheScheduledTimeIsReached()
	{
		var fakeTimeProvider = new FakeTimeProvider(StartInstant);
		var service = new CountingScheduledService(
			CreateScopeFactory(),
			fakeTimeProvider);

		// Advancing a full minute lands exactly on the next occurrence (08:01:00).
		fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
		await service.RunScheduleCheckAsync();

		Assert.Equal(1, service.ExecutionCount);
	}

	[Fact]
	public async Task FiresOncePerOccurrenceAsTheClockAdvances()
	{
		var fakeTimeProvider = new FakeTimeProvider(StartInstant);
		var service = new CountingScheduledService(
			CreateScopeFactory(),
			fakeTimeProvider);

		// First occurrence fires.
		fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
		await service.RunScheduleCheckAsync();
		Assert.Equal(1, service.ExecutionCount);

		// Checking again without advancing must not re-fire the same occurrence.
		await service.RunScheduleCheckAsync();
		Assert.Equal(1, service.ExecutionCount);

		// Reaching the next occurrence fires exactly once more.
		fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
		await service.RunScheduleCheckAsync();
		Assert.Equal(2, service.ExecutionCount);
	}

	[Fact]
	public async Task HostedLoopHonorsTheFakeClock()
	{
		var fakeTimeProvider = new FakeTimeProvider(StartInstant);
		var service = new CountingScheduledService(
			CreateScopeFactory(),
			fakeTimeProvider);
		IHostedService hostedService = service;

		await hostedService.StartAsync(CancellationToken.None);
		try
		{
			// The background loop is running on the real clock's cadence but reads
			// scheduling time from the fake provider; advancing it crosses the next
			// occurrence so an upcoming iteration executes the work.
			Assert.Equal(0, service.ExecutionCount);
			fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));

			await WaitForAsync(() => service.ExecutionCount >= 1);
			Assert.True(service.ExecutionCount >= 1);
		}
		finally
		{
			await hostedService.StopAsync(CancellationToken.None);
		}
	}

	private static async Task WaitForAsync(
		Func<bool> condition,
		int timeoutMilliseconds = 5000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
		while (DateTime.UtcNow < deadline)
		{
			if (condition())
			{
				return;
			}

			await Task.Delay(20);
		}

		throw new TimeoutException(
			"Condition was not satisfied within the allotted time.");
	}
}
