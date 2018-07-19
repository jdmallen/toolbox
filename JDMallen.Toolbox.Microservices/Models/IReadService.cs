﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.Microservices.Models
{
	/// <summary>
	/// Defines a service that can read <see cref="TEntityModel"/>s via a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The repository type used to perform the data actions</typeparam>
	/// <typeparam name="TEntityModel">The database entity model type</typeparam>
	/// <typeparam name="TQueryParameters">The type of object used to pass query parameters</typeparam>
	/// <typeparam name="TId">The primary key type</typeparam>
	public interface IReadService<out TRepository,
	                              TEntityModel,
	                              in TQueryParameters,
	                              in TId> : IService<TRepository>
		where TRepository : IReader<TEntityModel, TQueryParameters, TId>
		where TEntityModel : class, IEntityModel
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		/// <summary>
		/// Fetch a single <see cref="TEntityModel"/> via its <see cref="TId"/>
		/// </summary>
		/// <param name="id">The ID of the object to fetch</param>
		/// <returns>The fetched object</returns>
		Task<TEntityModel> Read(TId id);

		/// <summary>
		/// Fetch a single <see cref="TEntityModel"/> via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched object</returns>
		Task<TEntityModel> Find(TQueryParameters parameters);

		/// <summary>
		/// Fetch many <see cref="TEntityModel"/>s via a set of <see cref="TQueryParameters"/>
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects</returns>
		Task<IEnumerable<TEntityModel>> FindAll(TQueryParameters parameters);

		/// <summary> 
		/// Fetch many <see cref="TEntityModel"/>s via a set of <see cref="TQueryParameters"/> 
		/// and wrap the results in an <see cref="IPagedResult{TEntityModel}"/> suitable for UI pagination.
		/// </summary>
		/// <param name="parameters">The search parameters</param>
		/// <returns>The fetched list of objects in a paged result object</returns>
		Task<IPagedResult<TEntityModel>> FindAllPaged(TQueryParameters parameters);
	}
}