using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JDMallen.Toolbox.Hosting
{
	/// <summary>
	/// Adapted from https://thinkrethink.net/2018/02/21/asp-net-core-background-processing/
	/// </summary>
	/// <typeparam name="TService"></typeparam>
	public abstract class ScopedBackgroundService<TService> : BackgroundService
	{
		private readonly ILogger<TService> _logger;
		private readonly IServiceScopeFactory _scopeFactory;

		protected ScopedBackgroundService(
			ILogger<TService> logger,
			IServiceScopeFactory scopeFactory)
		{
			_logger = logger;
			_scopeFactory = scopeFactory;
		}

		protected abstract TimeSpan LoopDelay { get; }

		/// <summary>
		///   This method is called when the <see cref="IHostedService" /> starts and returns a task
		///   that represents the lifetime of the long running operation(s) being performed. In this
		///   specific implementation, it represents the looping operation, each execution being
		///   contained within a scope for the purpose of DI service lifetimes and logging. For
		///   direct subtypes of this class, <see cref="LoopDelay"/> represents times between
		///   executions. For implementations of the subtype
		///   <see cref="ScheduledBackgroundService{TService}"/>, <see cref="LoopDelay"/> represents
		///   how frequently the time is checked for the next scheduled occurrence.
		/// </summary>
		/// <param name="stoppingToken">
		///   Triggered when <see cref="BackgroundService.StopAsync" /> is called.
		/// </param>
		/// <returns>
		///   A <see cref="Task" /> that represents the long running operations.
		/// </returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			do
			{
				var sessionId = Guid.NewGuid();
				using IDisposable scope = _logger.BeginScope(sessionId);

				_logger.LogTrace("Begin service execution iteration");
				await ExecuteInScopeAsync(stoppingToken);
				_logger.LogTrace("End service execution iteration");

				await Task.Delay(LoopDelay, stoppingToken);
			}
			while (!stoppingToken.IsCancellationRequested);
		}

		protected virtual Task ExecuteInScopeAsync(CancellationToken stoppingToken)
		{
			using IServiceScope scope = _scopeFactory.CreateScope();
			return ExecuteInScopeAsync(scope, stoppingToken);
		}

		protected abstract Task ExecuteInScopeAsync(
			IServiceScope scope,
			CancellationToken stoppingToken);
	}
}
