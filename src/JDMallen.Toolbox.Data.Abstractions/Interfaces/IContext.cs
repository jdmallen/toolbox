using System.Data;

namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Represents a context from and to which the repository or service can
/// create, read, update, and delete data.
/// </summary>
public interface IContext : IDisposable
{
	/// <summary>
	/// Gets the underlying database connection for this context.
	/// </summary>
	/// <returns>The database connection.</returns>
	IDbConnection GetConnection();

	/// <summary>
	/// Asynchronously saves all changes made in this context to the database.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The number of state entries written to the database.</returns>
	Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default);
}
