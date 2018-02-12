using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Implementations
{
	public partial class Repository<TContext, TDomainModel, TEntityModel, TQueryParameters, TId>
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
	}
}