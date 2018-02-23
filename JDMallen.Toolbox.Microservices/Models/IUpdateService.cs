using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can update <see cref="TEntityModel"/>s via a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TEntityModel">The database entity model type</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface IUpdateService<out TRepository,
	                                TEntityModel,
	                                in TId> : IService<TRepository>
		where TRepository : IWriter<TEntityModel, TId>
		where TEntityModel : class, IEntityModel
		where TId : struct
	{
		/// <summary>
		/// Updates an existing <see cref="TEntityModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		Task<TEntityModel> Update(TEntityModel model);
	}
}
