using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext, TDomainModel, TEntityModel, TQueryParameters, TId>
		: IRepository<TDomainModel, TEntityModel>
		where TContext : IContext
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		public abstract Task<TDomainModel> Map(TEntityModel entity);

		public abstract Task<TEntityModel> Map(TDomainModel domainModel);

		public Task<TDomainModel[]> Map(IEnumerable<TEntityModel> entityModels)
			=> Task.WhenAll(entityModels.Select(Map));

		public Task<TEntityModel[]> Map(IEnumerable<TDomainModel> domainModels)
			=> Task.WhenAll(domainModels.Select(Map));
	}
}