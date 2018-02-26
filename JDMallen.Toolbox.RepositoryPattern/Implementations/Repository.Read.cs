using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext,
	                                         TEntityModel,
	                                         TQueryParameters,
	                                         TId>
		: IReader<TEntityModel, TQueryParameters, TId>
		where TContext : class, IContext
		where TEntityModel : class, IEntityModel
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		protected Repository(TContext context)
		{
			Context = context;
		}

		public TContext Context { get; }

		public abstract Task<bool> Any(TQueryParameters parameters);

		public abstract Task<long> Count(TQueryParameters parameters);

		public abstract Task<TEntityModel> Get(TId id);

		public abstract Task<TEntityModel> Find(TQueryParameters parameters);

		public abstract Task<List<TEntityModel>> FindAll(TQueryParameters parameters);

		public abstract Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters);
	}
}
