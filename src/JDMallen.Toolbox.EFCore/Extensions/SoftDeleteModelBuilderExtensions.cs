using System.Linq.Expressions;
using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JDMallen.Toolbox.EFCore.Extensions;

/// <summary>
/// <see cref="ModelBuilder" /> extensions that realize the soft-delete half of
/// the <see cref="IEntityModel" /> convention.
/// </summary>
public static class SoftDeleteModelBuilderExtensions
{
	/// <param name="modelBuilder">The model builder to configure.</param>
	extension(ModelBuilder modelBuilder)
	{
		/// <summary>
		/// Adds a global query filter of <c>e =&gt; !e.IsDeleted</c> to every mapped
		/// entity type implementing <see cref="IEntityModel" />, so soft-deleted rows
		/// are excluded from queries by default. Use
		/// <see
		///   cref="EntityFrameworkQueryableExtensions.IgnoreQueryFilters{TEntity}(System.Linq.IQueryable{TEntity})" />
		/// to opt back into seeing them.
		/// </summary>
		/// <returns>
		/// The same <paramref name="modelBuilder" /> instance, so calls can be chained.
		/// </returns>
		public ModelBuilder AddSoftDeleteQueryFilter()
		{
			foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
			{
				// A query filter may only be defined on the root of an inheritance
				// hierarchy, so derived types are skipped to avoid EF throwing.
				if (entityType.BaseType is not null
				    || !typeof(IEntityModel).IsAssignableFrom(entityType.ClrType))
				{
					continue;
				}

				// Build "e => !e.IsDeleted" for this specific entity CLR type.
				ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
				MemberExpression isDeletedProperty = Expression.Property(
					parameter,
					nameof(IEntityModel.IsDeleted));
				LambdaExpression notDeleted = Expression.Lambda(
					Expression.Not(isDeletedProperty),
					parameter);

				modelBuilder.Entity(entityType.ClrType).HasQueryFilter(notDeleted);
			}

			return modelBuilder;
		}

		/// <summary>
		/// Adds a global query filter of <c>e =&gt; !e.&lt;PropertyName&gt;</c> to every
		/// root, non-owned entity type that maps a <see cref="bool" /> property named
		/// <see cref="SoftDeleteFilterOptions.PropertyName" />, whether or not it
		/// implements <see cref="IEntityModel" />. Entity types without the property
		/// are skipped.
		/// </summary>
		/// <remarks>
		/// Call it at the end of <c>OnModelCreating</c>, after every entity type and
		/// property is configured; types added later aren't filtered.
		/// </remarks>
		/// <param name="options">The soft-delete property name and filter name.</param>
		/// <returns>
		/// The same <paramref name="modelBuilder" /> instance, so calls can be chained.
		/// </returns>
		public ModelBuilder AddSoftDeleteQueryFilter(SoftDeleteFilterOptions options)
		{
			ArgumentNullException.ThrowIfNull(options);
			ArgumentException.ThrowIfNullOrWhiteSpace(options.PropertyName);

			if (options.FilterName is not null)
			{
				ArgumentException.ThrowIfNullOrWhiteSpace(options.FilterName);
			}

			foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
			{
				// Filters are only allowed on hierarchy roots, and owned types are
				// filtered through their owner.
				if (entityType.BaseType is not null
				    || entityType.IsOwned()
				    || entityType.FindProperty(options.PropertyName)?.ClrType != typeof(bool))
				{
					continue;
				}

				// "e => !EF.Property<bool>(e, name)" also covers shadow properties.
				ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
				MethodCallExpression isDeletedProperty = Expression.Call(
					typeof(EF),
					nameof(EF.Property),
					[typeof(bool)],
					parameter,
					Expression.Constant(options.PropertyName));
				LambdaExpression notDeleted = Expression.Lambda(
					Expression.Not(isDeletedProperty),
					parameter);

				EntityTypeBuilder entityBuilder = modelBuilder.Entity(entityType.ClrType);

				if (options.FilterName is null)
				{
					entityBuilder.HasQueryFilter(notDeleted);
				}
				else
				{
					entityBuilder.HasQueryFilter(options.FilterName, notDeleted);
				}
			}

			return modelBuilder;
		}
	}
}
