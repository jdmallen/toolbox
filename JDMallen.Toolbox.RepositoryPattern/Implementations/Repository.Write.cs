using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext,
	                                         TDomainModel,
	                                         TEntityModel,
	                                         TQueryParameters,
	                                         TId>
		: IWriter<TDomainModel, TEntityModel, TId>
		where TContext : class, IContext
		where TDomainModel : class, IDomainModel
		where TEntityModel : class, IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		public abstract Task<TEntityModel> Add(TEntityModel model);

		public abstract Task<TEntityModel> Change(TEntityModel model);

		public abstract Task<TEntityModel> Remove(TId id);

		public abstract Task<int> SaveChanges(CancellationToken cancellationToken = default(CancellationToken));
	}
}
