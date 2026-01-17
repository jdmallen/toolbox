using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations;

public abstract partial class EFRepositoryBase<TContext, TEntityModel, TId>
	where TContext : DbContext, IContext
	where TEntityModel : class, IEntityModel<TId>
	where TId : struct
{
	/// <inheritdoc />
	public async Task<TEntityModel> AddAsync(TEntityModel model)
	{
		if (model == null)
			return null;
		var result = await Context.Set<TEntityModel>().AddAsync(model);
		await Context.SaveChangesAsync();
		return result.Entity;
	}

	/// <inheritdoc />
	public async Task<TEntityModel> UpdateAsync(TEntityModel model)
	{
		if (model == null)
			return null;
		var result = Context.Update(model);
		await Context.SaveChangesAsync();
		return result.Entity;
	}

	/// <inheritdoc />
	public Task Remove(TEntityModel model)
	{
		Context.Remove(model);
		return Context.SaveChangesAsync();
	}

	/// <summary>
	/// Saves all changes made in this context to the database.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The number of state entries written to the database.</returns>
	public Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		return Context.SaveChangesAsync(cancellationToken);
	}
}
