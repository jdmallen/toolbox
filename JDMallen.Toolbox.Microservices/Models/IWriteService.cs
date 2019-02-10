using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can create <see cref="TModel"/>s
	/// </summary>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	public interface IWriteService<TModel>
		: IService
		where TModel : class, IModel
	{
		/// <summary>
		/// Creates a new <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		Task<TModel> Upsert(TModel model);

		/// <summary>
		/// Updates an existing <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be created</param>
		/// <returns>The created object</returns>
		Task<TModel> Update(TModel model);

		/// <summary>
		/// Deletes an existing <see cref="TModel"/>
		/// </summary>
		/// <param name="model">The object to be deleted</param>
		/// <returns>The deleted object</returns>
		Task<TModel> Delete(TModel model);
	}
}
