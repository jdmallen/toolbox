using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models;

/// <summary>
/// Base class for SQLite-specific complex entity models that require additional
/// Entity Framework Core configuration.
/// </summary>
public abstract class SqliteComplexEntityModel
	: SqliteEntityModel, IComplexEntityModel
{
	/// <summary>
	/// Configures the SQLite-specific entity model using the Entity Framework Core
	/// ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	public abstract void OnModelCreating(ModelBuilder modelBuilder);
}

/// <summary>
/// Base class for SQLite-specific complex entity models with custom primary key
/// type
/// that require additional Entity Framework Core configuration.
/// </summary>
/// <typeparam name="TId">The type of the entity's primary key.</typeparam>
public abstract class SqliteComplexEntityModel<TId>
	: SqliteEntityModel<TId>, IComplexEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Configures the SQLite-specific entity model using the Entity Framework Core
	/// ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	public abstract void OnModelCreating(ModelBuilder modelBuilder);
}
