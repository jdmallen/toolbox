using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JDMallen.Toolbox.Hosting.TestWorker
{
	public class Worker : ScheduledBackgroundService<Worker>
	{
		private readonly ILogger<Worker> _logger;

		public Worker(
			ILogger<Worker> logger,
			IServiceScopeFactory scopeFactory) : base(logger, scopeFactory, "I am a bad schedule")
		{
			_logger = logger;
		}

		protected override TimeSpan LoopDelay => TimeSpan.FromSeconds(2);

		protected override async Task ExecuteInScopeAsync(
			IServiceScope scope,
			CancellationToken stoppingToken)
		{
			_logger.LogInformation("Worker running at: {time}", DateTime.Now);
		}
	}
}
