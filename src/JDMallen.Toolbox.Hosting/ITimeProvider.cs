namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// Abstraction for time operations to enable testability.
/// </summary>
/// <remarks>
/// This interface provides a testable abstraction over
/// <see cref="DateTime.UtcNow" />,
/// allowing background services to use deterministic time in unit tests.
/// For .NET 8+ projects, consider using the built-in TimeProvider class instead.
/// </remarks>
public interface ITimeProvider
{
	/// <summary>
	/// Gets the current UTC date and time.
	/// </summary>
	/// <returns>The current UTC date and time.</returns>
	DateTime GetUtcNow();
}
