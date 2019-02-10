using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IReader<TModel, in TQueryParameters, in TId> : IRepository
		where TModel : class, IModel
		where TQueryParameters : class
		where TId : struct
	{
		Task<bool> Any(TQueryParameters parameters);

		Task<long> Count(TQueryParameters parameters);

		Task<TModel> Get(TId id);

		Task<bool> Exists(TId id);

		Task<List<TModel>> Find(TQueryParameters parameters);

		Task<IPagedResult<TModel>> FindPaged(TQueryParameters parameters);
	}
}
