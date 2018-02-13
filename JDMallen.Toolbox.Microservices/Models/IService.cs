using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public interface IService
	{
	}

	public interface IService<out TRepository, TDomainModel, in TQueryParameters, in TId>
		where TRepository : IRepository
		where TDomainModel : IModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		TRepository Repository { get; }

		#region C

		Task<TDomainModel> Create(TDomainModel model);

		#endregion

		#region R
		Task<TDomainModel> Read(TId id);

		Task<TDomainModel> Find(TQueryParameters parameters);

		Task<IEnumerable<TDomainModel>> FindAll(TQueryParameters parameters);

		Task<IPagedResult<TDomainModel>> FindAllPaged(TQueryParameters parameters);

		#endregion

		#region U

		Task<TDomainModel> Update(TDomainModel model);

		#endregion

		#region D

		Task<TDomainModel> Delete(TDomainModel model);

		Task<TDomainModel> Delete(TId id);

		#endregion
	}
}