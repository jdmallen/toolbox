using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can delete <see cref="TEntityModel"/>s via a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TEntityModel">The database entity model type</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface IDeleteService<out TRepository,
	                                TEntityModel,
	                                in TId> : IService<TRepository>
		where TRepository : IWriter<TEntityModel, TId>
		where TEntityModel : class, IEntityModel
		where TId : struct
	{
		/// <summary>
		/// Deletes an existing <see cref="TEntityModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		Task<TEntityModel> Delete(TEntityModel model);

		/// <summary>
		/// Deletes an existing domain object from the data context via its repository
		/// </summary>
		/// <param name="id">The ID of the object to be deleted</param>
		/// <returns>The deleted object</returns>
		Task<TEntityModel> Delete(TId id);
	}
}
