﻿using System;

namespace JDMallen.Toolbox.Interfaces
{
	/// <summary>
	/// Represents a database entity, typically referred to by its respective SQL table.
	/// </summary>
	// ReSharper disable once InheritdocConsiderUsage
	public interface IEntityModel : IModel
	{
		DateTime DateCreated { get; }

		DateTime DateModified { get; }

		bool IsDeleted { get; }
	}

	/// <inheritdoc />
	/// <summary>
	/// This interface assumes the existence of a primary key of type <see cref="!:TId" /> 
	/// and two additional fields in the database entity: 
	/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateCreated" /> and 
	/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateModified" />.
	/// </summary>
	/// <typeparam name="TId">
	/// The type representing the data type for the primary key. This cannot be 
	/// <see cref="T:System.String" /> or any reference types. Only value types are allowed.
	/// </typeparam>
	public interface IEntityModel<out TId> : IEntityModel
		where TId : struct
	{
		TId Id { get; }
	}
}
