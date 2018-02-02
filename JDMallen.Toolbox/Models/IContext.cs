using System.Threading;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.Models
{
    public interface IContext
    {
	    Task<int> SaveAllChanges(CancellationToken cancellationToken = default(CancellationToken));
    }
}
