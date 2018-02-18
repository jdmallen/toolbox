using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class ReadWriteService<TRepository, TDomainModel, TEntityModel, TQueryParameters, TId>
		: ICreateService<TRepository, TDomainModel, TEntityModel, TId>,
		  IReadService<TRepository, TDomainModel, TEntityModel, TQueryParameters, TId>,
		  IUpdateService<TRepository, TDomainModel, TEntityModel, TId>,
		  IDeleteService<TRepository, TDomainModel, TEntityModel, TId>
		where TRepository : IReader<TDomainModel, TEntityModel, TQueryParameters, TId>, IWriter<TDomainModel, TEntityModel, TId>
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		public abstract TRepository Repository { get; }

		/// <summary>
		/// Creates a new <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public abstract Task<TDomainModel> Create(TDomainModel model);

		/// <summary>
		/// Fetch a single <see cref="TDomainModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		public abstract Task<TDomainModel> Read(TId id);

		/// <summary>
		/// Fetch a single <see cref="TDomainModel"/> via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched object</returns>
		public abstract Task<TDomainModel> Find(TQueryParameters parameters);

		/// <summary>
		/// Fetch many <see cref="TDomainModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		public abstract Task<IEnumerable<TDomainModel>> FindAll(TQueryParameters parameters);

		/// <summary> 
		/// Fetch many <see cref="TDomainModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TDomainModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		public abstract Task<IPagedResult<TDomainModel>> FindAllPaged(TQueryParameters parameters);

		/// <summary>
		/// Updates an existing <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public abstract Task<TDomainModel> Update(TDomainModel model);

		/// <summary>
		/// Deletes an existing <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		public abstract Task<TDomainModel> Delete(TDomainModel model);

		/// <summary>
		/// Deletes an existing domain object from the data context via its repository
		/// </summary>
		/// <param name="id">The ID of the object to be deleted</param>
		/// <returns>The deleted object</returns>
		public abstract Task<TDomainModel> Delete(TId id);
	}
}
