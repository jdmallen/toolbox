using JDMallen.Toolbox.Implementations;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models
{
	public abstract class SqliteComplexEntityModel
		: SqliteEntityModel, IComplexEntityModel
	{
		public abstract void OnModelCreating(ModelBuilder modelBuilder);
	}

	public abstract class SqliteComplexEntityModel<TId>
		: SqliteEntityModel<TId>, IComplexEntityModel<TId>
		where TId : struct
	{
		public abstract void OnModelCreating(ModelBuilder modelBuilder);
	}
}
