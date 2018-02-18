using JDMallen.Toolbox.Models;
using JDMallen.Toolbox.RepositoryPattern.Interfaces;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public partial class Repository<TContext, TDomainModel, TEntityModel, TQueryParameters, TId> : IRepository
		where TContext : IContext
		where TDomainModel : IDomainModel
		where TEntityModel : IEntityModel
		where TQueryParameters : IQueryParameters
		where TId : struct
	{
		protected Repository(TContext context)
		{
			Context = context;
		}

		public TContext Context { get; }
	}
}