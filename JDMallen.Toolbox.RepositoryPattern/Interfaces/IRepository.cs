using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
    public interface IRepository
    {
	    Task SaveChanges();
    }

	public interface IRepository<out TContext, TEntityModel, in TQueryParameters, in TId> : IRepository
		where TContext : IContext
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		TContext Context { get; }

		Task<bool> Any(TQueryParameters parameters);

		Task<long> Count(TQueryParameters parameters);
		
		Task<TEntityModel> Get(TId id);

		Task<ICollection<TEntityModel>> Find(TQueryParameters parameters);

		Task<IPagedResult<TEntityModel>> FindPaged(TQueryParameters parameters);

		Task<TEntityModel> Add(TEntityModel model);

		Task<TEntityModel> Change(TEntityModel model);

		Task<TEntityModel> Remove(TId id);
	}
}
