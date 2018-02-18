using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// An abstract service that can only read data from a data source
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TDomainModel">The domain model type</typeparam>
	/// <typeparam name="TEntityModel">The database entity model type</typeparam>
	/// <typeparam name="TQueryParameters">The type of object used to pass query parameters</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public abstract class ReadOnlyService<TRepository, TDomainModel, TEntityModel, TQueryParameters, TId>
		: IReadService<TRepository, TDomainModel, TEntityModel, TQueryParameters, TId>
		where TRepository : IReader<TDomainModel, TEntityModel, TQueryParameters, TId>
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		public abstract TRepository Repository { get; }

		/// <summary>
		/// Fetch a single <see cref="TDomainModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		public abstract Task<TDomainModel> Read(TId id);

		/// <summary>
		/// Fetch a single <see cref="TDomainModel"/> via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched object</returns>
		public abstract Task<TDomainModel> Find(TQueryParameters parameters);

		/// <summary>
		/// Fetch many <see cref="TDomainModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		public abstract Task<IEnumerable<TDomainModel>> FindAll(TQueryParameters parameters);

		/// <summary> 
		/// Fetch many <see cref="TDomainModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TDomainModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		public abstract Task<IPagedResult<TDomainModel>> FindAllPaged(TQueryParameters parameters);
	}
}
