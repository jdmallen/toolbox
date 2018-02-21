using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IWriter<TDomainModel, TEntityModel, in TId> : IRepository<TDomainModel, TEntityModel>
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TId : struct
	{
		Task<TEntityModel> Add(TEntityModel model);

		Task<TEntityModel> Change(TEntityModel model);

		Task<TEntityModel> Remove(TId id);

		Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken));
	}
}
