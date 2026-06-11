using JDMallen.Toolbox.EFCore.Tests.Infrastructure;
using Microsoft.Extensions.Time.Testing;

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Verifies that the base context's switch to
/// <c>ApplyConfigurationsFromAssembly</c> discovers and applies the colocated
/// <see cref="GadgetConfiguration" /> — the modern replacement for the old
/// reflection-based self-configuring model.
/// </summary>
public class EntityConfigurationDiscoveryTests
{
	[Fact]
	public void ApplyConfigurationsFromAssembly_AppliesEntityConfiguration()
	{
		using var context = GadgetContextFactory.Create(
			new FakeTimeProvider());

		var nameProperty = context.Model
			.FindEntityType(typeof(Gadget))!
			.FindProperty(nameof(Gadget.Name))!;

		// The max length only exists in the model if GadgetConfiguration was
		// discovered and applied via ApplyConfigurationsFromAssembly.
		Assert.Equal(GadgetConfiguration.NameMaxLength, nameProperty.GetMaxLength());
		Assert.False(nameProperty.IsNullable);
	}
}
