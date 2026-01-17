using System.Data;

namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Represents a context from and to which the repository or service can
/// create, read, update, and delete data.
/// </summary>
public interface IContext : IDisposable
{
	IDbConnection GetConnection();

	Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default);
}
