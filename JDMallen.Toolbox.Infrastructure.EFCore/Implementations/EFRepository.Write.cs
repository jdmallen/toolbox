using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TQueryParameters, TId>
		: IWriter<TEntityModel, TId>
		where TContext : class, IEFContext
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public Task<int> SaveChanges(
			CancellationToken cancellationToken = default(CancellationToken))
			=> Context.SaveChangesAsync(cancellationToken);

		public async Task<TEntityModel> Add(TEntityModel model)
		{
			if (model == null) return null;
			var result = Context.Add<TEntityModel,TId>(model);
			return (TEntityModel) result.Entity;
		}

		public async Task<TEntityModel> Update(TEntityModel model)
		{
			if (model == null) return null;
			// var modelToUpdate = await Context.BuildQuery<TEntityModel>()
			//                                  .Where(x => model.Id.Equals(x.Id))
			//                                  .SingleOrDefaultAsync();
			// CopyProps(ref modelToUpdate, ref model);
			var result = Context.Update<TEntityModel, TId>(model);
			return (TEntityModel) result.Entity;
		}

		public async Task<TEntityModel> Remove(TId id)
		{
			if (Equals(id, default(TId))) return null;
			var modelToDelete = await Context.BuildQuery<TEntityModel>()
			                                 .AsNoTracking()
			                                 .Where(x => id.Equals(x.Id))
			                                 .SingleOrDefaultAsync();
			if (modelToDelete == null) return null;
			var result = Context.Remove<TEntityModel, TId>(modelToDelete);
			return (TEntityModel) result.Entity;
		}

		public Task<TEntityModel> Remove(TEntityModel model)
			=> Remove(model.Id);
	}
}
