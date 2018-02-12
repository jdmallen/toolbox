using System.Linq;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepository<TDomainModel, TEntityModel, TQueryParameters, TId>
		where TDomainModel : class, IDomainModel<TId>
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public override Task<int> SaveChanges() => Context.SaveChangesAsync();

		public override async Task<TEntityModel> Add(TEntityModel model)
		{
			if (model == null) return null;
			var result = await Context.AddAsync(model);
			return result.Entity;
		}

		public override async Task<TEntityModel> Change(TEntityModel model)
		{
			if (model == null) return null;
			var modelToUpdate = await Context.GetQueryable<TEntityModel>()
			                                 .Where(x => model.Id.Equals(x.Id))
			                                 .SingleOrDefaultAsync();
			CopyProps(ref modelToUpdate, ref model);
			var result = Context.Update(modelToUpdate);
			return result.Entity;
		}

		public override async Task<TEntityModel> Remove(TId id)
		{
			if (Equals(id, default(TId))) return null;
			var modelToDelete = await Context.GetQueryable<TEntityModel>()
			                                 .Where(x => id.Equals(x.Id))
			                                 .SingleOrDefaultAsync();
			if (modelToDelete == null) return null;
			var result = Context.Remove(modelToDelete);
			return result.Entity;
		}
	}
}
