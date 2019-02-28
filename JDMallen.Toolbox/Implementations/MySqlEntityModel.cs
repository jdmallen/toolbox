using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Interfaces;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Implementations
{
	public abstract class MySqlEntityModel : IEntityModel
	{
		public DateTime DateCreated { get; protected set; } = DateTime.UtcNow;

		public DateTime DateModified { get; protected set;  } = DateTime.UtcNow;

		public bool IsDeleted { get; set; } = false;
	}

	public abstract class MySqlEntityModel<TId>
		: MySqlEntityModel, IEntityModel<TId>
		where TId : struct
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public TId Id { get; protected set; }

		[JsonIgnore]
		public string IdText
		{
			get => Id.ToString();
			protected set
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
