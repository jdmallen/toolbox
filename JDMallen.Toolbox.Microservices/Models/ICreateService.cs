using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can create <see cref="TModel"/>s via a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface ICreateService<out TRepository,
	                                TModel,
	                                in TId> : IService<TRepository>
		where TRepository : IWriter<TModel, TId>
		where TModel : class, IModel
		where TId : struct
	{
		/// <summary>
		/// Creates a new <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		Task<TModel> Create(TModel model);
	}
}
