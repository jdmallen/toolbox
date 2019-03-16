using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TId>
		where TContext : DbContext, IContext
		where TEntityModel : class, IEntityModel<TId>
		where TId : struct
	{
		public Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default(CancellationToken))
			=> Context.SaveChangesAsync(cancellationToken);

		public async Task<TEntityModel> AddAsync(TEntityModel model)
		{
			if (model == null)
				return null;
			var result = await Context.Set<TEntityModel>().AddAsync(model);
			await Context.SaveChangesAsync();
			return result.Entity;
		}

		public async Task<TEntityModel> UpdateAsync(TEntityModel model)
		{
			if (model == null)
				return null;
			var result = Context.Update(model);
			await Context.SaveChangesAsync();
			return result.Entity;
		}
		
		public Task Remove(TEntityModel model)
		{
			Context.Remove(model);
			return Context.SaveChangesAsync();
		}
	}
}
