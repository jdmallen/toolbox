using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can read <see cref="TModel"/>s
	/// </summary>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	/// <typeparam name="TQueryParameters">The type of object used to pass query parameters</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface IReadService<TModel,
	                              in TQueryParameters,
	                              in TId> : IService
		where TModel : class, IModel
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		/// <summary>
		/// Fetch a single <see cref="TModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		Task<TModel> Read(TId id);

		/// <summary>
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		Task<List<TModel>> Find(TQueryParameters parameters);

		/// <summary> 
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TEntityModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		Task<IPagedResult<TModel>> FindPaged(TQueryParameters parameters);
	}
}
