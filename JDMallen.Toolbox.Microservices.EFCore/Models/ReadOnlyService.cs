using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using JDMallen.Toolbox.Microservices.Models;

namespace JDMallen.Toolbox.Microservices.EFCore.Models
{
	/// <summary>
	/// An abstract service that can only read data from a data source
	/// </summary>
	/// <typeparam name="TRepository">The context used to perform the data actions</typeparam>
	/// <typeparam name="TModel">The database entity model type</typeparam>
	/// <typeparam name="TQueryParameters">The type of object used to pass query parameters</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public abstract class ReadOnlyService<TRepository, TModel, TQueryParameters,
	                                      TId>
		: IReadService<TModel, TQueryParameters, TId>
		where TRepository : IReader<TModel, TQueryParameters, TId>
		where TModel : class, IModel
		where TQueryParameters : class, IQueryParameters<TId>
		where TId : struct
	{
		/// <summary>
		/// Base constructor with DI context
		/// </summary>
		/// <param name="context">The context to use</param>
		protected ReadOnlyService(TRepository context)
		{
			Context = context;
		}

		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		public TRepository Context { get; }

		/// <summary>
		/// Fetch a single <see cref="TModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		public async Task<TModel> Read(TId id)
			=> await Context.Get(id);

		/// <summary>
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		public async Task<List<TModel>> Find(TQueryParameters parameters)
			=> await Context.Find(parameters);

		/// <summary> 
		/// Fetch many <see cref="TModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TEntityModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		public async Task<IPagedResult<TModel>> FindPaged(TQueryParameters parameters)
			=> await Context.FindPaged(parameters);
	}
}
