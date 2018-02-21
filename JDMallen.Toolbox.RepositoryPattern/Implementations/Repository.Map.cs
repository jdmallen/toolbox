using JDMallen.Toolbox.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext,
	                                         TDomainModel,
	                                         TEntityModel,
	                                         TQueryParameters,
	                                         TId>
		where TContext : class, IContext
		where TDomainModel : class, IDomainModel
		where TEntityModel : class, IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		protected Repository(TContext context)
		{
			Context = context;
		}

		public TContext Context { get; }

		public abstract Task<TDomainModel> Map(TEntityModel entity);

		public abstract Task<TEntityModel> Map(TDomainModel domainModel);

		public Task<TDomainModel[]> Map(IEnumerable<TEntityModel> entityModels)
			=> Task.WhenAll(entityModels.Select(Map));

		public Task<TEntityModel[]> Map(IEnumerable<TDomainModel> domainModels)
			=> Task.WhenAll(domainModels.Select(Map));
	}
}
