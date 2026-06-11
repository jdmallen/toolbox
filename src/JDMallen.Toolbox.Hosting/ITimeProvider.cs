#if !NET8_0_OR_GREATER
namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// Abstraction for time operations to enable testability.
/// </summary>
/// <remarks>
/// This interface provides a testable abstraction over
/// <see cref="DateTime.UtcNow" />, allowing background services to use
/// deterministic time in unit tests. On .NET 8+ the background service base
/// classes use the built-in <c>System.TimeProvider</c> instead, so this
/// interface is compiled only for the netstandard2.0 target.
/// </remarks>
public interface ITimeProvider
{
	/// <summary>
	/// Gets the current UTC date and time.
	/// </summary>
	/// <returns>The current UTC date and time.</returns>
	DateTime GetUtcNow();
}
#endif
