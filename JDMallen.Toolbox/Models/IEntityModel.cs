using System;

namespace JDMallen.Toolbox.Models
{
    public interface IEntityModel : IModel
    {
    }

	public interface IEntityModel<TId> : IEntityModel
	{
		TId Id { get; set; }

		DateTime DateCreated { get; set; }

		DateTime DateModified { get; set; }
	}
}
