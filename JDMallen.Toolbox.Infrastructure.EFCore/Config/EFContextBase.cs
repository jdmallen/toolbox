using System;
using System.Data;
using System.Linq;
using System.Threading;
using JDMallen.Toolbox.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JDMallen.Toolbox.EFCore.Config
{
	/// <inheritdoc cref="DbContext" />
	/// <summary>
	/// Custom abstract extension of <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> that allows for the 
	/// <see cref="T:Microsoft.EntityFrameworkCore.ModelBuilder" /> code to live inside the <see cref="T:JDMallen.Toolbox.EFCore.Models.IComplexEntityModel" />s 
	/// themselves, rather than in one massive method here.
	/// </summary>
	public abstract class EFContextBase : DbContext, IEFContext
	{
		/// <inheritdoc />
		/// <param name="options"></param>
		protected EFContextBase(DbContextOptions options) : base(options)
		{
		}

		/// <summary>
		/// Source: https://stackoverflow.com/a/48346941/3986790
		/// </summary>
		/// <remarks>
		/// This special version of OnModelCreating finds all classes implementing
		/// <see cref="IEntityModel"/> and adds them to the central EF model.
		/// After that, it then finds all models with their own OnModelCreating
		/// method (those implementing <see cref="IComplexEntityModel"/>) and
		/// executes that method on each entity model in succession.
		/// This allows the user to place the ModelBuilder code
		/// in each entity class instead of all lumped together here.
		/// </remarks>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			GetType()
				.Assembly
				.DefinedTypes
				// Get all the types that implement
				.Where(type => typeof(IEntityModel).IsAssignableFrom(type)
							   // IEntityModel and are concrete classes
							   && !type.IsAbstract
							   && !type.IsInterface)
				.ToList()	 // Compile them all into a list
				.ForEach(type =>
				{
					// And add each one to the ModelBuilder.
					if (modelBuilder.Model.FindEntityType(type) != null) return;
					// If it's already been added, skip it.
					modelBuilder.Model.AddEntityType(type);
				});

			GetType()
				.Assembly
				.DefinedTypes
				// Get all the types that implement
				.Where(type => typeof(IComplexEntityModel).IsAssignableFrom(type)
							   // IComplexEntityModel and are concrete classes
							   && !type.IsAbstract
							   && !type.IsInterface)
				// Instantiate each 
				.Select(Activator.CreateInstance)
				// as an IComplexEntityModel
				.Cast<IComplexEntityModel>()
				.ToList()
				// And call each's respective OnModelCreating method.
				.ForEach(model => model.OnModelCreating(modelBuilder));
		}

		public EntityEntry<TEntityModel> Add<TEntityModel, TId>(
			TEntityModel model,
			CancellationToken cancellationToken = default(CancellationToken))
			where TEntityModel : class, IEntityModel<TId> 
			where TId : struct
			=> base.Add(model);

		public IQueryable<TEntityModel> BuildQuery<TEntityModel>()
			where TEntityModel : class, IEntityModel
			=> Set<TEntityModel>();

		public EntityEntry<TEntityModel> Entry<TEntityModel, TId>(TEntityModel model)
			where TEntityModel: class, IEntityModel<TId>
			where TId : struct
			=> Entry(model);

		public EntityEntry Update<TEntityModel, TId>(TEntityModel modelToUpdate)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct
			=> Update(modelToUpdate);

		public EntityEntry Remove<TEntityModel, TId>(TEntityModel modelToDelete)
			where TEntityModel : class, IEntityModel<TId> 
			where TId : struct
			=> Remove(modelToDelete);

		public IDbConnection GetConnection() => base.Database.GetDbConnection();
	}
}
