using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Implementations
{
	public abstract partial class EFRepository<TContext, TDomainModel, TEntityModel, TQueryParameters, TId>
		where TContext : class, IEFContext
		where TDomainModel : class, IDomainModel<TId>
		where TEntityModel : class, IEntityModel<TId>
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public override Task<int> SaveChanges(
			CancellationToken cancellationToken = default(CancellationToken))
			=> Context.SaveAllChanges(cancellationToken);

		public override async Task<TEntityModel> Add(TEntityModel model)
		{
			if (model == null) return null;
			var result = await Context.AddAsync<TEntityModel,TId>(model);
			return (TEntityModel) result.Entity;
		}

		public override async Task<TEntityModel> Change(TEntityModel model)
		{
			if (model == null) return null;
			var modelToUpdate = await Context.BuildQuery<TEntityModel>()
			                                 .Where(x => model.Id.Equals(x.Id))
			                                 .SingleOrDefaultAsync();
			CopyProps(ref modelToUpdate, ref model);
			var result = Context.Update<TEntityModel, TId>(modelToUpdate);
			return (TEntityModel) result.Entity;
		}

		public override async Task<TEntityModel> Remove(TId id)
		{
			if (Equals(id, default(TId))) return null;
			var modelToDelete = await Context.BuildQuery<TEntityModel>()
			                                 .Where(x => id.Equals(x.Id))
			                                 .SingleOrDefaultAsync();
			if (modelToDelete == null) return null;
			var result = Context.Remove<TEntityModel, TId>(modelToDelete);
			return (TEntityModel) result.Entity;
		}
	}
}
