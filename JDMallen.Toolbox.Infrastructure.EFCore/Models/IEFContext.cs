using System.Linq;
using System.Threading.Tasks;
using JDMallen.Toolbox.Models;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Models
{
	public interface IEFContext : IContext
	{
		Task<IQueryable<TEntityModel>> BuildQuery<TEntityModel>();
	}
}