using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JDMallen.Toolbox.Interfaces;
using Newtonsoft.Json;

namespace JDMallen.Toolbox.Implementations
{
	public abstract class MySqlEntityModel : EntityModel
	{
	}

	public abstract class MySqlEntityModel<TId>
		: EntityModel<TId>, IEntityModel<TId>
		where TId : struct
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public new TId Id { get; set; }

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
