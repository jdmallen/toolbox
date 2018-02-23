 using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class WriteOnlyService<TRepository, TEntityModel, TId>
		: ICreateService<TRepository, TEntityModel, TId>,
		IUpdateService<TRepository, TEntityModel, TId>,
		IDeleteService<TRepository, TEntityModel, TId>
		where TRepository : IWriter<TEntityModel, TId>
		where TEntityModel : class, IEntityModel<TId>
		where TId : struct
	{
		/// <summary>
		/// Base constructor with DI repository
		/// </summary>
		/// <param name="repository">The repository to use</param>
		protected WriteOnlyService(TRepository repository)
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
