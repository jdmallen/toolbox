using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Structs;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Models
{
	public abstract class MySqlEntityModel : IEntityModel
	{
		public DateTime DateCreated { get; set; }

		public DateTime DateModified { get; set; }
	}

	public abstract class MySqlEntityModel<TId>
		: MySqlEntityModel, IEntityModel<TId>
		where TId : struct
	{
		private TId _id;

		[Key]
		public TId Id { get => _id; set => _id = value; }

		public string IdText => Id.ToString();

		[NotMapped]
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
