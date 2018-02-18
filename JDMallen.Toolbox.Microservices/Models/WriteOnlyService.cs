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
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
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
