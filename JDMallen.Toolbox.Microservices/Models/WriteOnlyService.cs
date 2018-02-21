 using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	public abstract class WriteOnlyService<TRepository, TDomainModel, TEntityModel, TId>
		: ICreateService<TRepository, TDomainModel, TEntityModel, TId>,
		  IUpdateService<TRepository, TDomainModel, TEntityModel, TId>,
		  IDeleteService<TRepository, TDomainModel, TEntityModel, TId>
		where TRepository : IWriter<TDomainModel, TEntityModel, TId>
		where TDomainModel : IDomainModel<TId>
		where TEntityModel : IEntityModel
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
		/// Creates a new <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TDomainModel> Create(TDomainModel model) 
			=> await Repository.Map(await Repository.Add(await Repository.Map(model)));

		/// <summary>
		/// Updates an existing <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		public async Task<TDomainModel> Update(TDomainModel model)
			=> await Repository.Map(await Repository.Change(await Repository.Map(model)));

		/// <summary>
		/// Deletes an existing <see cref="TDomainModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		public Task<TDomainModel> Delete(TDomainModel model)
			=> Delete(model.Id);

		/// <summary>
		/// Deletes an existing domain object from the data context via its repository
		/// </summary>
		/// <param name="id">The ID of the object to be deleted</param>
		/// <returns>The deleted object</returns>
		public async Task<TDomainModel> Delete(TId id)
			=> await Repository.Map(await Repository.Remove(id));
	}
}
