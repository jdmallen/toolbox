using JDMallen.Toolbox.EFCore.Patterns.Repository.Interfaces;

namespace JDMallen.Toolbox.EFCore.Services.Interfaces
{
	/// <summary>
	/// Defines a service that can read from a given repository
	/// </summary>
	/// <typeparam name="TRepository"></typeparam>
	public interface IReadService<out TRepository> : IService<TRepository>
		where TRepository : IRepository
	{
	}
}
