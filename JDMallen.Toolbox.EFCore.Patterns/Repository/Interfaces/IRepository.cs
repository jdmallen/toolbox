using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JDMallen.Toolbox.EFCore.Patterns.Specification.Interfaces;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces
{
	public interface IRepository
	{
	}

	public interface IRepository<TModel, in TId> : IRepository, IDisposable
		where TModel : class, IModel
		where TId : struct
	{
		Task<bool> AnyBySpecAsync(ISpecification<TModel> spec);

		Task<long> CountBySpecAsync(ISpecification<TModel> spec);

		ValueTask<TModel> GetByIdAsync(TId id);

		Task<bool> ExistsByIdAsync(TId id);

		IAsyncEnumerable<TModel> FindBySpecAsync(
			ISpecification<TModel> spec);

		IAsyncEnumerable<TModel> ListAllAsync();

		Task<TModel> AddAsync(TModel model);

		Task<TModel> UpdateAsync(TModel model);

		Task Remove(TModel model);
	}
}
