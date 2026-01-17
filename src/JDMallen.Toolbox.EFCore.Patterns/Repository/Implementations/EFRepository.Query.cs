using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations;

/// <summary>
/// Base class for Entity Framework Core repository implementations.
/// </summary>
/// <typeparam name="TContext">The Entity Framework Core DbContext type.</typeparam>
/// <typeparam name="TEntityModel">The entity model type managed by this repository.</typeparam>
/// <typeparam name="TId">The primary key type of the entity.</typeparam>
public abstract partial class EFRepositoryBase<
	TContext,
	TEntityModel,
	TId> : IRepository<TEntityModel, TId>
	where TContext : DbContext, IContext
	where TEntityModel : class, IEntityModel<TId>
	where TId : struct
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EFRepositoryBase{TContext, TEntityModel, TId}"/> class.
	/// </summary>
	/// <param name="context">The Entity Framework Core context.</param>
	protected EFRepositoryBase(TContext context)
	{
		Context = context;
	}

	/// <summary>
	/// Gets the Entity Framework Core context used by this repository.
	/// </summary>
	protected TContext Context { get; }

	/// <summary>
	/// Applies a specification to a queryable to filter and include related entities.
	/// </summary>
	/// <param name="specification">The specification containing filters and includes.</param>
	/// <returns>A queryable with the specification applied.</returns>
	private IQueryable<TEntityModel> ApplySpecification(
		ISpecification<TEntityModel> specification)
	{
		return QueryBuilder<TEntityModel>.Build(
			Context.Set<TEntityModel>().AsQueryable(),
			specification);
	}

	#region IDisposable

	private bool _disposed;

	/// <summary>
	/// Releases the resources used by the repository.
	/// </summary>
	/// <param name="disposing">
	/// If true, releases both managed and unmanaged resources.
	/// If false, releases only unmanaged resources.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
			if (disposing)
				Context.Dispose();

		_disposed = true;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	#endregion
}
