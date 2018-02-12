using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IMapper<out TContext, TDomainModel, TEntityModel> 
		: IRepository<TContext>
		where TContext : IContext
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
	{
		Task<TDomainModel> Map(TEntityModel entity);

		Task<TEntityModel> Map(TDomainModel domainModel);

		Task<TDomainModel[]> Map(IEnumerable<TEntityModel> entityModels);

		Task<TEntityModel[]> Map(IEnumerable<TDomainModel> domainModels);
	}
}
