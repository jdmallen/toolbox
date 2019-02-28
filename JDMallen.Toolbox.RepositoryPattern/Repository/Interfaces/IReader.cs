using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces
{
	public interface IReader<TModel, in TId> : IRepository
		where TModel : class, IModel
		where TId : struct
	{
		Task<bool> AnyBySpecAsync(ISpecification<TModel> spec);

		Task<long> CountBySpecAsync(ISpecification<TModel> spec);

		Task<TModel> GetByIdAsync(TId id);

		Task<bool> ExistsByIdAsync(TId id);

		IAsyncEnumerable<TModel> FindBySpecAsync(
			ISpecification<TModel> spec);

		IAsyncEnumerable<TModel> ListAllAsync();
	}
}
