using JDMallen.Toolbox.EFCore.Config;
using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JDMallen.Toolbox.EFCore.Tests.Infrastructure;

/// <summary>
/// A minimal entity used to exercise the <see cref="EFContextBase" /> conventions:
/// it carries the <see cref="EntityModel{TId}" /> audit/soft-delete fields and a
/// single payload property configured via <see cref="GadgetConfiguration" />.
/// </summary>
public sealed class Gadget : EntityModel<Guid>
{
	public string Name { get; set; } = string.Empty;
}

/// <summary>
/// An <see cref="IEntityTypeConfiguration{TEntity}" /> for <see cref="Gadget" />,
/// present purely so the tests can prove
/// <see
///   cref="ModelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly,System.Func{System.Type,bool})" />
/// discovered and applied it.
/// </summary>
public sealed class GadgetConfiguration : IEntityTypeConfiguration<Gadget>
{
	internal const int NameMaxLength = 64;

	public void Configure(EntityTypeBuilder<Gadget> builder)
	{
		builder.Property(gadget => gadget.Name)
			.HasMaxLength(NameMaxLength)
			.IsRequired();
	}
}

/// <summary>
/// Concrete <see cref="EFContextBase" /> used by the tests. It overrides
/// <see cref="EFContextBase.TimeProvider" /> so the auto-registered audit
/// interceptor stamps a controllable clock.
/// </summary>
public sealed class GadgetContext(DbContextOptions options, TimeProvider clock)
	: EFContextBase(options)
{
	public DbSet<Gadget> Gadgets => Set<Gadget>();

	protected override TimeProvider TimeProvider { get; } = clock;
}
