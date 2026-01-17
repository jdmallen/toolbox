using System;

namespace JDMallen.Toolbox.Hosting;

/// <summary>
///   Default implementation of <see cref="ITimeProvider" /> that uses <see cref="DateTime.UtcNow" />.
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
	/// <summary>
	///   Gets the singleton instance of <see cref="SystemTimeProvider" />.
	/// </summary>
	public static SystemTimeProvider Instance { get; } = new();

	/// <inheritdoc />
	public DateTime GetUtcNow() => DateTime.UtcNow;
}
