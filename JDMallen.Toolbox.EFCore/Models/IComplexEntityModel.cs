using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models
{
	public interface IComplexEntityModel : IEntityModel
	{
		void OnModelCreating(ModelBuilder modelBuilder);
	}

	public interface IComplexEntityModel<TId> : IEntityModel<TId>, IComplexEntityModel
	{
	}
}