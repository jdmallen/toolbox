using System.Threading.Tasks;
using JDMallen.Toolbox.Interfaces;

namespace JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces
{
	public interface IWriter<TModel> : IRepository
		where TModel : class, IModel
	{
		Task<TModel> AddAsync(TModel model);

		Task<TModel> UpdateAsync(TModel model);

		Task Remove(TModel model);
	}
}
