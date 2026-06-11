using System.Linq.Expressions;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Extensions;

/// <summary>
/// <see cref="ModelBuilder" /> extensions that realize the soft-delete half of
/// the <see cref="IEntityModel" /> convention.
/// </summary>
public static class SoftDeleteModelBuilderExtensions
{
	/// <summary>
	/// Adds a global query filter of <c>e =&gt; !e.IsDeleted</c> to every mapped
	/// entity type implementing <see cref="IEntityModel" />, so soft-deleted rows
	/// are excluded from queries by default. Use
	/// <see cref="EntityFrameworkQueryableExtensions.IgnoreQueryFilters{TEntity}(System.Linq.IQueryable{TEntity})" />
	/// to opt back into seeing them.
	/// </summary>
	/// <param name="modelBuilder">The model builder to configure.</param>
	/// <returns>
	/// The same <paramref name="modelBuilder" /> instance, so calls can be chained.
	/// </returns>
	public static ModelBuilder AddSoftDeleteQueryFilter(this ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			// A query filter may only be defined on the root of an inheritance
			// hierarchy, so derived types are skipped to avoid EF throwing.
			if (entityType.BaseType is not null
			    || !typeof(IEntityModel).IsAssignableFrom(entityType.ClrType))
			{
				continue;
			}

			// Build "e => !e.IsDeleted" for this specific entity CLR type.
			var parameter = Expression.Parameter(entityType.ClrType, "e");
			var isDeletedProperty = Expression.Property(
				parameter,
				nameof(IEntityModel.IsDeleted));
			var notDeleted = Expression.Lambda(
				Expression.Not(isDeletedProperty),
				parameter);

			modelBuilder.Entity(entityType.ClrType).HasQueryFilter(notDeleted);
		}

		return modelBuilder;
	}
}
