using System;

namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// Represents a domain model, typically populated with properties of more
	/// complex types than its entity ancestor.
	/// </summary>
	// ReSharper disable once InheritdocConsiderUsage
	public interface IDomainModel : IModel
	{
	}

	/// <inheritdoc />
	/// <summary>
	/// This interface assumes the existence of an Id field of type <see cref="TId"/>
	/// and two additional nullable fields: 
	/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateCreated" /> and 
	/// <see cref="P:JDMallen.Toolbox.Models.IEntityModel`1.DateModified" />.
	/// The domain model need not necessarily correspond with a matching
	/// <see cref="IEntityModel"/>.
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	public interface IDomainModel<TId> : IDomainModel
		where TId : struct
	{
		TId Id { get; set; }

		DateTime? DateCreated { get; set; }

		DateTime? DateModified { get; set; }
	}
}
