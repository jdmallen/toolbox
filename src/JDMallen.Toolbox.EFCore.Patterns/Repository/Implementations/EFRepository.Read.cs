using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations;

public abstract partial class EFRepositoryBase<TContext, TEntityModel, TId>
	where TContext : DbContext, IContext
	where TEntityModel : class, IEntityModel<TId>
	where TId : struct
{
	/// <inheritdoc />
	public Task<bool> AnyBySpecAsync(ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).AnyAsync();
	}

	/// <inheritdoc />
	public Task<long> CountBySpecAsync(ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).LongCountAsync();
	}

	/// <inheritdoc />
	public ValueTask<TEntityModel?> GetByIdAsync(TId id)
	{
		return Context.FindAsync<TEntityModel>(id);
	}

	/// <inheritdoc />
	public Task<bool> ExistsByIdAsync(TId id)
	{
		return Context.Set<TEntityModel>().AnyAsync(x => x.Id.Equals(id));
	}

	/// <inheritdoc />
	public IAsyncEnumerable<TEntityModel> FindBySpecAsync(
		ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).AsAsyncEnumerable();
	}

	/// <inheritdoc />
	public IAsyncEnumerable<TEntityModel> ListAllAsync()
	{
		return Context.Set<TEntityModel>().AsAsyncEnumerable();
	}
}
