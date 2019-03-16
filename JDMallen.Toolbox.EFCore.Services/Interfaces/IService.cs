using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;

namespace JDMallen.Toolbox.EFCore.Services.Interfaces
{
	/// <summary>
	/// Defines a service used to implement business logic
	/// </summary>
	public interface IService
	{
	}

	/// <summary>
	/// Defines a service used to implement business logic using a given <see cref="TRepository"/>
	/// </summary>
	/// <typeparam name="TRepository">The type of repository to use</typeparam>
	public interface IService<out TRepository> : IService
		where TRepository : IRepository
	{
		/// <summary>
		/// The <see cref="TRepository"/> used to perform all the CRUD actions
		/// </summary>
		TRepository Repository { get; }
	}
}
