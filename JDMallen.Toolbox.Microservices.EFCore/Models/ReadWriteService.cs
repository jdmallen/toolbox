using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Microservices.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.EFCore.Models
{
	public abstract class ReadWriteService<TRepository, TModel, TQueryParameters, TId>
		: IWriteService<TModel>,
		IReadService<TModel, TQueryParameters, TId>
		where TRepository : IReader<TModel, TQueryParameters, TId>, IWriter<TModel, TId>
		where TModel : class, IModel
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
		/// Creates a new <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TModel> Upsert(TModel model)
			=> await Repository.Add(model);

		/// <summary>
		/// Fetch a single <see cref="TModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		public async Task<TModel> Read(TId id)
			=> await Repository.Get(id);

		/// <summary>
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		public async Task<List<TModel>> Find(TQueryParameters parameters)
			=> await Repository.Find(parameters);

		/// <summary> 
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TEntityModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		public async Task<IPagedResult<TModel>> FindPaged(TQueryParameters parameters)
			=> await Repository.FindPaged(parameters);

		/// <summary>
		/// Updates an existing <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TModel> Update(TModel model)
			=> await Repository.Update(model);

		/// <summary>
		/// Deletes an existing <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		public async Task<TModel> Delete(TModel model)
			=> await Repository.Remove(model);
	}
}
