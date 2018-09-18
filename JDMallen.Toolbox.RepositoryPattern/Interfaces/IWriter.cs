using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.RepositoryPattern.Interfaces
{
	public interface IWriter<TModel, in TId> : IRepository
		where TModel : class, IModel
		where TId : struct
	{
		Task<TModel> Add(TModel model);

		Task<TModel> Update(TModel model);

		Task<TModel> Remove(TModel model);

		Task<TModel> Remove(TId id);
	}
}
