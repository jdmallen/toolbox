using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Models
{
	public interface IEFContext : IContext
	{
		Task<EntityEntry<TEntityModel>> AddAsync<TEntityModel, TId>(
			TEntityModel model,
			CancellationToken cancellationToken = default(CancellationToken))
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;

		IQueryable<TEntityModel> BuildQuery<TEntityModel>()
			where TEntityModel: class, IEntityModel;

		EntityEntry Update<TEntityModel, TId>(TEntityModel modelToUpdate)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;

		EntityEntry Remove<TEntityModel, TId>(TEntityModel modelToDelete)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct;
	}
}