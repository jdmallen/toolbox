using System;

namespace JDMallen.Toolbox.Models
{
    public interface IDomainModel : IModel
	{
    }

	public interface IDomainModel<TId> : IDomainModel
	{
		TId Id { get; set; }

		DateTime? DateCreated { get; set; }

		DateTime? DateModified { get; set; }
	}
}
