using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TQueryParameters, TId>
		: IWriter<TEntityModel, TId>
		where TContext : DbContext, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default(CancellationToken))
			=> Context.SaveChangesAsync(cancellationToken);

		public async Task<TEntityModel> Add(TEntityModel model)
		{
			if (model == null)
				return null;
			var result = Context.Add(model);
			return (TEntityModel) result.Entity;
		}

		public async Task<TEntityModel> Update(TEntityModel model)
		{
			if (model == null)
				return null;
			Set.Attach(model);
			var result = Context.Update(model);
			return (TEntityModel) result.Entity;
		}

		public async Task<TEntityModel> Remove(TId id)
		{
			if (Equals(id, default(TId)))
				return null;
			var modelToDelete = await Context.FindAsync<TEntityModel>(id);
			if (modelToDelete == null)
				return null;
			return await Remove(modelToDelete);
		}

		public async Task<TEntityModel> Remove(TEntityModel model)
		{
			if (Context.Entry(model).State == EntityState.Detached)
			{
				Set.Attach(model);
			}

			var result = Context.Remove(model);
			return result.Entity;
		}
	}
}
