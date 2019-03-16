using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;

namespace JDMallen.Toolbox.EFCore.Services.Interfaces
{
	/// <summary>
	/// Defines a service that can create entities via a given repository.
	/// </summary>
	/// <typeparam name="TRepository"></typeparam>
	public interface IWriteService<out TRepository> : IService<TRepository>
		where TRepository : class, IRepository
	{
	}
}
