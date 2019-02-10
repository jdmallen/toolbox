using System;
using System.ComponentModel;
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
		public TId Id { get; set; }

		public DateTime DateCreated { get; set; }

		public DateTime DateModified { get; set; }

		public bool IsDeleted { get; set; }

		public string IdText
		{
			get => Id.ToString();
			set
			{
				try
				{
					var converter = TypeDescriptor.GetConverter(typeof(TId));
					Id = (TId) converter.ConvertFromString(value);
				}
				catch
				{
					Id = default(TId);
				}
			}
		}
	}
}
