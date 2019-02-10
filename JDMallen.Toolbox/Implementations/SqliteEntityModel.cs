using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.Implementations
{
	public abstract class SqliteEntityModel : IEntityModel
	{
		public DateTime DateCreated { get; set; }

		public DateTime DateModified { get; set; }

		public bool IsDeleted { get; set; }
	}

	public abstract class SqliteEntityModel<TId>
		: SqliteEntityModel, IEntityModel<TId>
		where TId : struct
	{
		[Key]
		public TId Id { get; set; }

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
