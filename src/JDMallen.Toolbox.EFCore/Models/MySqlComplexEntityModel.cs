using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models;

/// <summary>
/// Base class for MySQL-specific complex entity models that require additional
/// Entity Framework Core configuration.
/// </summary>
public abstract class MySqlComplexEntityModel
	: MySqlEntityModel, IComplexEntityModel
{
	/// <summary>
	/// Configures the MySQL-specific entity model using the Entity Framework Core
	/// ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	public abstract void OnModelCreating(ModelBuilder modelBuilder);
}

/// <summary>
/// Base class for MySQL-specific complex entity models with custom primary key
/// type
/// that require additional Entity Framework Core configuration.
/// </summary>
/// <typeparam name="TId">The type of the entity's primary key.</typeparam>
public abstract class MySqlComplexEntityModel<TId>
	: MySqlEntityModel<TId>, IComplexEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Configures the MySQL-specific entity model using the Entity Framework Core
	/// ModelBuilder.
	/// </summary>
	/// <param name="modelBuilder">
	/// The builder used to configure the entity in the
	/// database context.
	/// </param>
	public abstract void OnModelCreating(ModelBuilder modelBuilder);
}
