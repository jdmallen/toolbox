using System.Threading.Tasks;
using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public abstract partial class Repository<TContext, TDomainModel, TEntityModel, TQueryParameters, TId>
		: IWriter<TContext, TEntityModel, TId>
		where TContext : IContext
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		public abstract Task<int> SaveChanges();

		public TContext Context { get; }

		public abstract Task<TEntityModel> Add(TEntityModel model);

		public abstract Task<TEntityModel> Change(TEntityModel model);

		public abstract Task<TEntityModel> Remove(TId id);
	}
}
