using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models;

/// <inheritdoc />
/// <summary>
/// This model, special for Entity Framework Core, assumes the entity requires
/// additional
/// configuration.
/// </summary>
public interface IComplexEntityModel : IEntityModel
{
	/// <summary>
	/// Configures the entity model using the Entity Framework Core ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	void OnModelCreating(ModelBuilder modelBuilder);
}

/// <inheritdoc cref="IComplexEntityModel" />
/// <inheritdoc cref="IEntityModel{TId}" />
/// <summary>
/// </summary>
/// <typeparam name="TId"></typeparam>
public interface IComplexEntityModel<out TId>
	: IEntityModel<TId>, IComplexEntityModel
	where TId : struct
{
}

/// <summary>
/// Base class for complex entity models with custom primary key type that require
/// additional Entity Framework Core configuration.
/// </summary>
/// <typeparam name="TId">The type of the entity's primary key.</typeparam>
public abstract class ComplexEntityModel<TId>
	: EntityModel<TId>, IComplexEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Configures the entity model using the Entity Framework Core ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	public abstract void OnModelCreating(ModelBuilder modelBuilder);
}
