namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Represents a context from and to which data can be created, read, updated,
/// and deleted.
/// </summary>
public interface IContext : IDisposable
{
	/// <summary>
	/// Asynchronously saves all changes made in this context to the database.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
	/// <returns>The number of state entries written to the database.</returns>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
