using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests.Infrastructure;

/// <summary>
/// Builds <see cref="GadgetContext" /> instances backed by an isolated in-memory
/// store and a shared <see cref="FakeTimeProvider" />, so each test gets a clean
/// database while driving the audit clock deterministically.
/// </summary>
internal static class GadgetContextFactory
{
	public static GadgetContext Create(FakeTimeProvider clock)
	{
		var options = new DbContextOptionsBuilder<GadgetContext>()
			// A unique database name per call keeps tests isolated from each other.
			.UseInMemoryDatabase($"gadgets-{Guid.NewGuid()}")
			.Options;

		return new GadgetContext(options, clock);
	}
}
