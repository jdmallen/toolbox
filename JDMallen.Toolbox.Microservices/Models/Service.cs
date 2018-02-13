using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class Service<TRepository, TDomainModel, TQueryParameters, TId>
		: IService<TRepository, TDomainModel, TQueryParameters, TId>
		where TRepository : IRepository
		where TDomainModel : IModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		protected Service(TRepository repository)
		{
			Repository = repository;
		}

		public TRepository Repository { get; }

		public abstract Task<TDomainModel> Create(TDomainModel model);

		public abstract Task<TDomainModel> Read(TId id);

		public abstract Task<TDomainModel> Find(TQueryParameters parameters);

		public abstract Task<IEnumerable<TDomainModel>> FindAll(TQueryParameters parameters);

		public abstract Task<IPagedResult<TDomainModel>> FindAllPaged(TQueryParameters parameters);

		public abstract Task<TDomainModel> Update(TDomainModel model);

		public abstract Task<TDomainModel> Delete(TDomainModel model);

		public abstract Task<TDomainModel> Delete(TId id);
	}
}
