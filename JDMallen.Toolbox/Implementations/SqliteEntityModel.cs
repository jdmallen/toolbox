using System.ComponentModel;

namespace JDMallen.Toolbox.Implementations
{
	public abstract class SqliteEntityModel : EntityModel
	{
	}

	public abstract class SqliteEntityModel<TId>
		: EntityModel<TId>
		where TId : struct
	{
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
