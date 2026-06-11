namespace JDMallen.Toolbox.Hosting.TestWorker;

/// <summary>
/// The main entry point for the hosted worker service application.
/// </summary>
public class Program
{
	/// <summary>
	/// Creates and configures the host builder for the worker service.
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	/// <returns>A configured <see cref="IHostBuilder" />.</returns>
	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.ConfigureServices((_, services) => { services.AddHostedService<Worker>(); });
	}

	/// <summary>
	/// The application entry point that creates and runs the host.
	/// </summary>
	/// <param name="args">Command-line arguments.</param>
	public static void Main(string[] args)
	{
		CreateHostBuilder(args).Build().Run();
	}
}
