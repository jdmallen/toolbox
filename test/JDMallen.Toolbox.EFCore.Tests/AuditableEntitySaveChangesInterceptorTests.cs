using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Verifies that the audit interceptor auto-registered by
/// <see cref="JDMallen.Toolbox.EFCore.Config.EFContextBase" /> stamps the
/// <c>DateCreated</c>/<c>DateModified</c> fields of saved
/// <see cref="JDMallen.Toolbox.Data.Abstractions.Interfaces.IEntityModel" />
/// entities from the context's clock.
/// </summary>
public class AuditableEntitySaveChangesInterceptorTests
{
	private static readonly DateTimeOffset StartInstant =
		new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

	[Fact]
	public async Task AddedEntity_StampsCreatedAndModifiedFromTheClock()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using var context = GadgetContextFactory.Create(clock);

		var gadget = new Gadget { Name = "Widget" };
		context.Gadgets.Add(gadget);
		await context.SaveChangesAsync();

		Assert.Equal(StartInstant.UtcDateTime, gadget.DateCreated);
		Assert.Equal(StartInstant.UtcDateTime, gadget.DateModified);
	}

	[Fact]
	public async Task ModifiedEntity_AdvancesModifiedButPreservesCreated()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using var context = GadgetContextFactory.Create(clock);

		var gadget = new Gadget { Name = "Widget" };
		context.Gadgets.Add(gadget);
		await context.SaveChangesAsync();

		// Advance the clock, then mutate and save again.
		clock.Advance(TimeSpan.FromHours(1));
		gadget.Name = "Widget Mk II";
		await context.SaveChangesAsync();

		var expectedModified = StartInstant.UtcDateTime.AddHours(1);
		Assert.Equal(expectedModified, gadget.DateModified);
		// DateCreated must remain the original creation instant.
		Assert.Equal(StartInstant.UtcDateTime, gadget.DateCreated);
	}
}
