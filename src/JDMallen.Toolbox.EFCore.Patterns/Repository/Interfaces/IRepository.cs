using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;

/// <summary>
/// Marker interface for repository implementations.
/// </summary>
public interface IRepository
{
}

/// <summary>
/// Generic repository interface for data access operations on entity models.
/// </summary>
/// <typeparam name="TModel">The entity model type.</typeparam>
/// <typeparam name="TId">The primary key type of the entity.</typeparam>
public interface IRepository<TModel, in TId> : IRepository, IDisposable
	where TModel : class, IModel
	where TId : struct
{
	/// <summary>
	/// Determines whether any entities satisfy the given specification.
	/// </summary>
	/// <param name="spec">The specification to apply.</param>
	/// <returns>True if any entities match the specification, otherwise false.</returns>
	Task<bool> AnyBySpecAsync(ISpecification<TModel> spec);

	/// <summary>
	/// Counts the number of entities that satisfy the given specification.
	/// </summary>
	/// <param name="spec">The specification to apply.</param>
	/// <returns>The count of matching entities.</returns>
	Task<long> CountBySpecAsync(ISpecification<TModel> spec);

	/// <summary>
	/// Retrieves an entity by its primary key.
	/// </summary>
	/// <param name="id">The primary key value.</param>
	/// <returns>The entity with the specified ID, or null if not found.</returns>
	ValueTask<TModel> GetByIdAsync(TId id);

	/// <summary>
	/// Determines whether an entity with the specified ID exists.
	/// </summary>
	/// <param name="id">The primary key value.</param>
	/// <returns>True if an entity with the specified ID exists, otherwise false.</returns>
	Task<bool> ExistsByIdAsync(TId id);

	/// <summary>
	/// Finds all entities that satisfy the given specification.
	/// </summary>
	/// <param name="spec">The specification to apply.</param>
	/// <returns>An async enumerable of entities matching the specification.</returns>
	IAsyncEnumerable<TModel> FindBySpecAsync(
		ISpecification<TModel> spec);

	/// <summary>
	/// Retrieves all entities from the repository.
	/// </summary>
	/// <returns>An async enumerable of all entities.</returns>
	IAsyncEnumerable<TModel> ListAllAsync();

	/// <summary>
	/// Adds a new entity to the repository.
	/// </summary>
	/// <param name="model">The entity to add.</param>
	/// <returns>The added entity with any generated values populated.</returns>
	Task<TModel> AddAsync(TModel model);

	/// <summary>
	/// Updates an existing entity in the repository.
	/// </summary>
	/// <param name="model">The entity to update.</param>
	/// <returns>The updated entity.</returns>
	Task<TModel> UpdateAsync(TModel model);

	/// <summary>
	/// Removes an entity from the repository.
	/// </summary>
	/// <param name="model">The entity to remove.</param>
	Task Remove(TModel model);
}
