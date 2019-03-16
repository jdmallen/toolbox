using System;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.Implementations
{
	public abstract class EntityModel : IEntityModel
	{
		public DateTime DateCreated { get; set; } = DateTime.UtcNow;

		public DateTime DateModified { get; set; } = DateTime.UtcNow;

		public bool IsDeleted { get; set; } = false;
	}

	/// <summary>
	/// Represents a database entity, typically referred to by its respective SQL table.
	/// </summary>
	// ReSharper disable once InheritdocConsiderUsage
	public abstract class EntityModel<TId> : EntityModel, IEntityModel<TId>
		where TId : struct
	{
		public TId Id { get; set; }
	}
}
