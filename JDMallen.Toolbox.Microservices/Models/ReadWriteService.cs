using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class ReadWriteService<TRepository, TEntityModel, TQueryParameters, TId>
		: ICreateService<TRepository, TEntityModel, TId>,
		IReadService<TRepository, TEntityModel, TQueryParameters, TId>,
		IUpdateService<TRepository, TEntityModel, TId>,
		IDeleteService<TRepository, TEntityModel, TId>
		where TRepository : IReader<TEntityModel, TQueryParameters, TId>, IWriter<TEntityModel, TId>
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		/// <summary>
		/// Base constructor with DI repository
		/// </summary>
		/// <param name="repository">The repository to use</param>
		protected ReadWriteService(TRepository repository)
		{
			Repository = repository;
		}

		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		public TRepository Repository { get; }

		/// <summary>
		/// Creates a new <see cref="TEntityModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TEntityModel> Create(TEntityModel model)
			=> await Repository.Add(model);

		/// <summary>
		/// Fetch a single <see cref="TEntityModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		public async Task<TEntityModel> Read(TId id)
			=> await Repository.Get(id);

		/// <summary>
		/// Fetch a single <see cref="TEntityModel"/> via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched object</returns>
		public async Task<TEntityModel> Find(TQueryParameters parameters)
			=> await Repository.Find(parameters);

		/// <summary>
		/// Fetch many <see cref="TEntityModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		public async Task<IEnumerable<TEntityModel>> FindAll(TQueryParameters parameters)
			=> await Repository.FindAll(parameters);

		/// <summary> 
		/// Fetch many <see cref="TEntityModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TEntityModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		public async Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters)
			=> await Repository.FindAllPaged(parameters);

		/// <summary>
		/// Updates an existing <see cref="TEntityModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TEntityModel> Update(TEntityModel model)
			=> await Repository.Change(model);

		/// <summary>
		/// Deletes an existing <see cref="TEntityModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		public Task<TEntityModel> Delete(TEntityModel model)
			=> Delete(model.Id);

		/// <summary>
		/// Deletes an existing domain object from the data context via its repository
		/// </summary>
		/// <param name="id">The ID of the object to be deleted</param>
		/// <returns>The deleted object</returns>
		public async Task<TEntityModel> Delete(TId id)
			=> await Repository.Remove(id);
	}
}
