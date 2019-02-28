using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Models
{
	/// <inheritdoc />
	/// <summary>
	/// This model, special for Entity Framework Core, assumes the entity requires additional 
	/// configuration.
	/// </summary>
	public interface IComplexEntityModel : IEntityModel
	{
		void OnModelCreating(ModelBuilder modelBuilder);
	}

	/// <inheritdoc cref="IComplexEntityModel" />
	/// <inheritdoc cref="IEntityModel{TId}"/>
	/// <summary>
	/// </summary>
	/// <typeparam name="TId"></typeparam>
	public interface IComplexEntityModel<TId> : IEntityModel<TId>, IComplexEntityModel
		where TId : struct
	{
	}
}