using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IReader<TDomainModel, TEntityModel, in TQueryParameters, in TId> : IRepository<TDomainModel, TEntityModel>
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		Task<bool> Any(TQueryParameters parameters);

		Task<long> Count(TQueryParameters parameters);

		Task<TEntityModel> Get(TId id);

		Task<TEntityModel> Find(TQueryParameters parameters);

		Task<List<TEntityModel>> FindAll(TQueryParameters parameters);

		Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters);
	}
}
