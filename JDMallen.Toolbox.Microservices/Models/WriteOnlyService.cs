 using System.Threading.Tasks;
 using JDMallen.Toolbox.Interfaces;
 using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class WriteOnlyService<TRepository, TModel, TId>
		: ICreateService<TRepository, TModel, TId>,
		IUpdateService<TRepository, TModel, TId>,
		IDeleteService<TRepository, TModel, TId>
		where TRepository : IWriter<TModel, TId>
		where TModel : class, IModel
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
		/// Creates a new <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TModel> Create(TModel model)
			=> await Repository.Add(model);

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

		/// <summary>
		/// Deletes an existing domain object from the data context via its repository
		/// </summary>
		/// <param name="id">The ID of the object to be deleted</param>
		/// <returns>The deleted object</returns>
		public async Task<TModel> Delete(TId id)
			=> await Repository.Remove(id);
	}
}
