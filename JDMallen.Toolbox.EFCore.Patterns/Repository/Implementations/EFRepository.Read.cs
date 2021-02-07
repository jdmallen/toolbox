using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Implementations
{
	public abstract partial class EFRepositoryBase<TContext, TEntityModel, TId>
		where TContext : DbContext, IContext
		where TEntityModel : class, IEntityModel<TId>
		where TId : struct
	{
		public Task<bool> AnyBySpecAsync(ISpecification<TEntityModel> specification)
			=> ApplySpecification(specification).AnyAsync();

		public Task<long> CountBySpecAsync(ISpecification<TEntityModel> specification)
			=> ApplySpecification(specification).LongCountAsync();

		public ValueTask<TEntityModel> GetByIdAsync(TId id)
			=> Context.FindAsync<TEntityModel>(id);

		public Task<bool> ExistsByIdAsync(TId id)
			=> Context.Set<TEntityModel>().AnyAsync(x => x.Id.Equals(id));

		public IAsyncEnumerable<TEntityModel> FindBySpecAsync(
			ISpecification<TEntityModel> specification)
			=> ApplySpecification(specification).AsAsyncEnumerable();

		public IAsyncEnumerable<TEntityModel> ListAllAsync()
			=> Context.Set<TEntityModel>().AsAsyncEnumerable();
	}
}
