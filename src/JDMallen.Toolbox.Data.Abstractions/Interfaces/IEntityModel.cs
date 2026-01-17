namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Represents a database entity, typically referred to by its respective SQL
/// table.
/// </summary>
// ReSharper disable once InheritdocConsiderUsage
public interface IEntityModel : IModel
{
	/// <summary>
	/// Gets the date and time when the entity was created.
	/// </summary>
	DateTime DateCreated { get; }

	/// <summary>
	/// Gets the date and time when the entity was last modified.
	/// </summary>
	DateTime DateModified { get; }

	/// <summary>
	/// Gets a value indicating whether the entity has been soft-deleted.
	/// </summary>
	bool IsDeleted { get; }
}

/// <inheritdoc />
/// <summary>
/// This interface assumes the existence of a primary key of type
/// <see cref="!:TId" />
/// and two additional fields in the database entity:
/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateCreated" /> and
/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateModified" />.
/// </summary>
/// <typeparam name="TId">
/// The type representing the data type for the primary key. This cannot be
/// <see cref="T:System.String" /> or any reference types. Only value types are
/// allowed.
/// </typeparam>
public interface IEntityModel<out TId> : IEntityModel
	where TId : struct
{
	/// <summary>
	/// Gets the primary key identifier for the entity.
	/// </summary>
	TId Id { get; }
}
