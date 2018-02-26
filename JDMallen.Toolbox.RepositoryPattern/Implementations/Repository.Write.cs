using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext,
	                                         TEntityModel,
	                                         TQueryParameters,
	                                         TId>
		: IWriter<TEntityModel, TId>
		where TContext : class, IContext
		where TEntityModel : class, IEntityModel
		where TQueryParameters : class, IQueryParameters
		where TId : struct
	{
		public abstract Task<TEntityModel> Add(TEntityModel model);

		public abstract Task<TEntityModel> Change(TEntityModel model);

		public abstract Task<TEntityModel> Remove(TId id);

		public abstract Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken));
	}
}
