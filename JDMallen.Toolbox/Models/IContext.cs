using System.Threading;
using System.Threading.Tasks;

namespace JDMallen.Toolbox.Models
{
	/// <summary>
	/// Represents a context from and to which the repository or service can 
	/// create, read, update, and delete data.
	/// </summary>
    public interface IContext
    {
		Task<int> SaveAllChanges(CancellationToken cancellationToken = default(CancellationToken));
	}
}
