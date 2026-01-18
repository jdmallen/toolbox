using JetBrains.Annotations;

namespace JDMallen.Toolbox.Models;

/// <summary>
/// Represents the state of a unit of work transaction.
/// </summary>
[UsedImplicitly]
public enum UnitOfWorkState
{
	/// <summary>
	/// The transaction is open and changes can be made.
	/// </summary>
	Open,

	/// <summary>
	/// The transaction has been successfully committed.
	/// </summary>
	Committed,

	/// <summary>
	/// The transaction has been rolled back, discarding all changes.
	/// </summary>
	RolledBack,

	/// <summary>
	/// The transaction has been disposed.
	/// </summary>
	Disposed
}