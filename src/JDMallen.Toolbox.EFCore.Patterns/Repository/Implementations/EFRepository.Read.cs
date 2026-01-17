using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations;

public abstract partial class EFRepositoryBase<TContext, TEntityModel, TId>
	where TContext : DbContext, IContext
	where TEntityModel : class, IEntityModel<TId>
	where TId : struct
{
	public Task<bool> AnyBySpecAsync(ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).AnyAsync();
	}

	public Task<long> CountBySpecAsync(ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).LongCountAsync();
	}

	public ValueTask<TEntityModel> GetByIdAsync(TId id)
	{
		return Context.FindAsync<TEntityModel>(id);
	}

	public Task<bool> ExistsByIdAsync(TId id)
	{
		return Context.Set<TEntityModel>().AnyAsync(x => x.Id.Equals(id));
	}

	public IAsyncEnumerable<TEntityModel> FindBySpecAsync(
		ISpecification<TEntityModel> specification)
	{
		return ApplySpecification(specification).AsAsyncEnumerable();
	}

	public IAsyncEnumerable<TEntityModel> ListAllAsync()
	{
		return Context.Set<TEntityModel>().AsAsyncEnumerable();
	}
}
