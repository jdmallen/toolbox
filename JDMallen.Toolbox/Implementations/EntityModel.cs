using System;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Structs;

namespace JDMallen.Toolbox.Implementations
{
	/// <summary>
	/// Represents a database entity, typically referred to by its respective SQL table.
	/// </summary>
	// ReSharper disable once InheritdocConsiderUsage
	public abstract class EntityModel<TId> : IEntityModel<TId>
		where TId : struct
	{
		private TId _id;

		public TId Id { get => _id; set => _id = value; }

		public DateTime DateCreated { get; set; }

		public DateTime DateModified { get; set; }

		public string IdText => Id.ToString();

		public MiniGuid ShortId
		{
			get
			{
				if (typeof(TId) != typeof(Guid))
					throw new NotSupportedException("ShortId only available for Guid IDs.");
				return MiniGuid.Encode((Guid)(object)_id);
			}
			set
			{
				if (typeof(TId) != typeof(Guid))
					throw new NotSupportedException("ShortId only available for Guid IDs.");
				_id = (TId)(object) MiniGuid.Decode(value.ToString());
			}
		}
	}
}
