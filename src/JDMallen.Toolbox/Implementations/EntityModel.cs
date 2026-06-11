using JDMallen.Toolbox.Data.Abstractions.Interfaces;

namespace JDMallen.Toolbox.Implementations;

/// <summary>
/// Base class for entity models providing common properties for audit tracking and
/// soft deletes.
/// </summary>
public abstract class EntityModel : IEntityModel
{
	/// <summary>
	/// Gets or sets the date and time when the entity was created.
	/// </summary>
	public DateTime DateCreated { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Gets or sets the date and time when the entity was last modified.
	/// </summary>
	public DateTime DateModified { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// Gets or sets a value indicating whether the entity is logically deleted (soft
	/// delete).
	/// </summary>
	public bool IsDeleted { get; set; }
}

/// <summary>
/// Generic base class for database entities with a primary key of type
/// <typeparamref name="TId" />.
/// Typically used for entities referred to by their respective SQL table.
/// </summary>
/// <typeparam name="TId">The type of the primary key.</typeparam>

// ReSharper disable once InheritdocConsiderUsage
public abstract class EntityModel<TId> : EntityModel, IEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Gets or sets the primary key of the entity.
	/// </summary>
	public TId Id { get; protected set; }
}
