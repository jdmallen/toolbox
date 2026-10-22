using JDMallen.Toolbox.Data.Abstractions.Interfaces;

namespace JDMallen.Toolbox.EFCore.Extensions;

/// <summary>
/// Configures
/// <see
///   cref="SoftDeleteModelBuilderExtensions.AddSoftDeleteQueryFilter(Microsoft.EntityFrameworkCore.ModelBuilder, SoftDeleteFilterOptions)" />
/// for entities that don't implement <see cref="IEntityModel" />.
/// </summary>
/// <remarks>
/// Every root, non-owned entity type with a mapped <see cref="bool" /> property
/// named <see cref="PropertyName" /> (CLR or shadow) gets the filter; entity
/// types without one are left unfiltered.
/// </remarks>
public sealed record SoftDeleteFilterOptions
{
	/// <summary>
	/// The <see cref="bool" /> property that marks a row as deleted. Defaults to
	/// <see cref="IEntityModel.IsDeleted" />'s name.
	/// </summary>
	public string PropertyName { get; init; } = nameof(IEntityModel.IsDeleted);

	/// <summary>
	/// Registers the filter as an EF Core named query filter, so a query can drop
	/// just this one with <c>IgnoreQueryFilters([FilterName])</c> while keeping
	/// any others on the entity. <see langword="null" /> (the default) registers
	/// an unnamed filter, as the parameterless overload does; EF Core doesn't
	/// allow named and unnamed filters on the same entity type.
	/// </summary>
	public string? FilterName { get; init; }
}
