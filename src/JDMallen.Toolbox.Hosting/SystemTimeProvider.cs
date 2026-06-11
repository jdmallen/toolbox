#if !NET8_0_OR_GREATER
namespace JDMallen.Toolbox.Hosting;

/// <summary>
/// Default implementation of <see cref="ITimeProvider" /> that uses
/// <see cref="DateTime.UtcNow" />. Compiled only for targets that lack the
/// built-in <c>System.TimeProvider</c> (i.e., netstandard2.0).
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
	/// <summary>
	/// Gets the singleton instance of <see cref="SystemTimeProvider" />.
	/// </summary>
	public static SystemTimeProvider Instance { get; } = new();

	/// <inheritdoc />
	public DateTime GetUtcNow()
	{
		return DateTime.UtcNow;
	}
}
#endif
