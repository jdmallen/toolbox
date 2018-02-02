using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Config
{
	public abstract class EFContextBase : DbContext, IContext
	{
		protected EFContextBase(DbContextOptions options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			GetType()
				.Assembly
				.DefinedTypes
				.Where(type => typeof(IEntityModel).IsAssignableFrom(type)
				               && !type.IsAbstract
				               && !type.IsInterface)
				.ToList()
				.ForEach(type =>
				{
					if (modelBuilder.Model.FindEntityType(type) != null) return;

					modelBuilder.Model.AddEntityType(type);
				});
		}

		public Task<int> SaveAllChanges(CancellationToken cancellationToken = default(CancellationToken))
		{
			return SaveChangesAsync(cancellationToken);
		}
	}
}
