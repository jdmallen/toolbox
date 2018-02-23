using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IWriter<TEntityModel, in TId> : IRepository
		where TEntityModel : class, IEntityModel
		where TId : struct
	{
		Task<TEntityModel> Add(TEntityModel model);

		Task<TEntityModel> Change(TEntityModel model);

		Task<TEntityModel> Remove(TId id);

		Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken));
	}
}
