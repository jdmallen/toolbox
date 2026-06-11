using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Verifies the soft-delete global query filter applied by
/// <see cref="JDMallen.Toolbox.EFCore.Extensions.SoftDeleteModelBuilderExtensions.AddSoftDeleteQueryFilter" />
/// (wired up automatically by the base context): soft-deleted rows are hidden by
/// default and revealed only when query filters are ignored.
/// </summary>
public class SoftDeleteQueryFilterTests
{
	private static readonly DateTimeOffset Instant =
		new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

	private static async Task<GadgetContext> SeededContextAsync()
	{
		var context = GadgetContextFactory.Create(new FakeTimeProvider(Instant));

		context.Gadgets.Add(new Gadget { Name = "Visible" });
		context.Gadgets.Add(new Gadget { Name = "Hidden", IsDeleted = true });
		await context.SaveChangesAsync();

		return context;
	}

	[Fact]
	public async Task SoftDeletedEntities_AreExcludedByDefault()
	{
		await using var context = await SeededContextAsync();

		var names = await context.Gadgets
			.Select(gadget => gadget.Name)
			.ToListAsync();

		Assert.Equal(["Visible"], names);
	}

	[Fact]
	public async Task IgnoreQueryFilters_RevealsSoftDeletedEntities()
	{
		await using var context = await SeededContextAsync();

		var names = await context.Gadgets
			.IgnoreQueryFilters()
			.Select(gadget => gadget.Name)
			.OrderBy(name => name)
			.ToListAsync();

		Assert.Equal(["Hidden", "Visible"], names);
	}
}
