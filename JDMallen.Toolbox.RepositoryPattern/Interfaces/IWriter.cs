using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IWriter<out TContext, TEntityModel, in TId>
		: IRepository<TContext>
		where TContext : IContext
		where TEntityModel : IEntityModel
		where TId : struct
	{
		Task<int> SaveChanges();

		Task<TEntityModel> Add(TEntityModel model);

		Task<TEntityModel> Change(TEntityModel model);

		Task<TEntityModel> Remove(TId id);
	}
}
