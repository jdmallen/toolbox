using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models
{
	public abstract class MySqlComplexEntityModel
		: MySqlEntityModel, IComplexEntityModel
	{
		public abstract void OnModelCreating(ModelBuilder modelBuilder);
	}

	public abstract class MySqlComplexEntityModel<TId>
		: MySqlEntityModel<TId>, IComplexEntityModel<TId>
		where TId : struct
	{
		public abstract void OnModelCreating(ModelBuilder modelBuilder);
	}
}
