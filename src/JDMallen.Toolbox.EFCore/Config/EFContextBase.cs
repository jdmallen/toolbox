using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Extensions;
using JDMallen.Toolbox.EFCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Config;

/// <inheritdoc cref="DbContext" />
/// <summary>
/// Abstract base <see cref="DbContext" /> that wires up the conventions this
/// toolbox provides around <see cref="IEntityModel" />:
/// <list type="bullet">
/// <item>
/// entity configuration is discovered from
/// <see cref="IEntityTypeConfiguration{TEntity}" /> implementations in the
/// derived context's assembly via
/// <see cref="ModelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly,System.Func{System.Type,bool})" />;
/// </item>
/// <item>
/// a soft-delete global query filter is applied through
/// <see cref="SoftDeleteModelBuilderExtensions.AddSoftDeleteQueryFilter" />;
/// </item>
/// <item>
/// audit timestamps are stamped on save by
/// <see cref="AuditableEntitySaveChangesInterceptor" />.
/// </item>
/// </list>
/// </summary>
public abstract class EFContextBase : DbContext, IContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EFContextBase" /> class.
	/// </summary>
	/// <param name="options">The Entity Framework Core context options.</param>
	protected EFContextBase(DbContextOptions options) : base(options)
	{
	}

	/// <summary>
	/// Gets the clock used by the audit interceptor to stamp timestamps. Override
	/// to supply a deterministic clock (for example, a fake
	/// <see cref="TimeProvider" /> in tests). Defaults to
	/// <see cref="TimeProvider.System" />.
	/// </summary>
	protected virtual TimeProvider TimeProvider => TimeProvider.System;

	/// <inheritdoc />
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);

		// Registering the interceptor here means audit stamping works out of the
		// box for any derived context without extra DI wiring.
		optionsBuilder.AddInterceptors(
			new AuditableEntitySaveChangesInterceptor(TimeProvider));
	}

	/// <inheritdoc />
	/// <remarks>
	/// Applies entity configurations colocated with the model as
	/// <see cref="IEntityTypeConfiguration{TEntity}" /> implementations, then the
	/// soft-delete query filter for every <see cref="IEntityModel" />. Override and
	/// call <c>base.OnModelCreating(modelBuilder)</c> to add further configuration.
	/// </remarks>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
		modelBuilder.AddSoftDeleteQueryFilter();
	}
}
