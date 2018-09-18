using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Models
{
	public interface IEFContext : IContext
	{
		EntityEntry<TEntityModel> Add<TEntityModel, TId>(
			TEntityModel model,
			CancellationToken cancellationToken = default(CancellationToken))
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;

		IQueryable<TEntityModel> BuildQuery<TEntityModel>()
			where TEntityModel: class, IEntityModel;

		EntityEntry<TEntityModel> Entry<TEntityModel>(TEntityModel model)
			where TEntityModel: class;

		EntityEntry Update<TEntityModel, TId>(TEntityModel modelToUpdate)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;

		EntityEntry Remove<TEntityModel, TId>(TEntityModel modelToDelete)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;

		Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default(CancellationToken));
	}
}
