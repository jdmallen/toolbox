using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext,
	                                         TDomainModel,
	                                         TEntityModel,
	                                         TQueryParameters,
	                                         TId>
		: IReader<TDomainModel, TEntityModel, TQueryParameters, TId>
		where TContext : class, IContext
		where TDomainModel : class, IDomainModel
		where TEntityModel : class, IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		public abstract Task<bool> Any(TQueryParameters parameters);

		public abstract Task<long> Count(TQueryParameters parameters);

		public abstract Task<TEntityModel> Get(TId id);

		public abstract Task<TEntityModel> Find(TQueryParameters parameters);

		public abstract Task<List<TEntityModel>> FindAll(TQueryParameters parameters);

		public abstract Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters);
	}
}
