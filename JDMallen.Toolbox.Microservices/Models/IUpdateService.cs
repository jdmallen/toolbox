using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can update <see cref="TModel"/>s via a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface IUpdateService<out TRepository,
	                                TModel,
	                                in TId> : IService<TRepository>
		where TRepository : IWriter<TModel, TId>
		where TModel : class, IModel
		where TId : struct
	{
		/// <summary>
		/// Updates an existing <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		Task<TModel> Update(TModel model);
	}
}
