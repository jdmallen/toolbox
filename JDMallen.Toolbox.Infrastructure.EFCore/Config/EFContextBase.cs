using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDMallen.Toolbox.Infrastructure.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using JDMallen.Toolbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JDMallen.Toolbox.Infrastructure.EFCore.Config
{
	/// <inheritdoc cref="DbContext" />
	/// <summary>
	/// Custom abstract extension of <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> that allows for the 
	/// <see cref="T:Microsoft.EntityFrameworkCore.ModelBuilder" /> code to live inside the <see cref="T:JDMallen.Toolbox.Infrastructure.EFCore.Models.IComplexEntityModel" />s 
	/// themselves, rather than in one massive method here.
	/// </summary>
	public abstract class EFContextBase : DbContext, IEFContext
	{
		/// <inheritdoc />
		/// <param name="options"></param>
		protected EFContextBase(DbContextOptions options) : base(options)
		{
		}

		public IQueryable<TEntityModel> GetQueryable<TEntityModel>()
			where TEntityModel : class, IEntityModel
		{
			return Set<TEntityModel>();
		}

		/// <summary>
		/// Source: https://stackoverflow.com/a/48346941/3986790
		/// </summary>
		/// <remarks>
		/// This special version of OnModelCreating finds all classes implementing 
		/// <see cref="IEntityModel"/> and adds them to the central EF model. After that,
		/// it then finds all models with their own OnModelCreating method (those
		/// implementing <see cref="IComplexEntityModel"/>) and executes that method on each
		/// entity model in succession. This allows the user to place the ModelBuilder code
		/// in each entity class instead of all lumped together here.
		/// </remarks>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			GetType()
				.Assembly
				.DefinedTypes
				.Where(type => typeof(IEntityModel).IsAssignableFrom(type)  // Get all the types that implement
							   && !type.IsAbstract                          // IEntityModel and are concrete classes
							   && !type.IsInterface)
				.ToList()                                                   // Compile them all into a list
				.ForEach(type =>
				{
					if (modelBuilder.Model.FindEntityType(type) != null) return;  // And add each one to the ModelBuilder.
					modelBuilder.Model.AddEntityType(type);                       // If it's already been added, skip it.
				});

			GetType()
				.Assembly
				.DefinedTypes
				.Where(type => typeof(IComplexEntityModel).IsAssignableFrom(type)  // Get all the types that implement
							   && !type.IsAbstract                                 // IComplexEntityModel and are concrete classes
							   && !type.IsInterface)
				.Select(Activator.CreateInstance)                           // Instantiate each 
				.Cast<IComplexEntityModel>()                                // as an IComplexEntityModel
				.ToList()
				.ForEach(model => model.OnModelCreating(modelBuilder));     // And call each's respective OnModelCreating method.
		}

		/// <summary>
		/// Save all changes to the DbContext. 
		/// Calls SaveChangesAsync on <see cref="DbContext"/>.
		/// </summary>
		/// <param name="cancellationToken">
		/// Optional async cancellation token.
		/// </param>
		/// <returns>Number of items changed in database.</returns>
		public Task<int> SaveAllChanges(CancellationToken cancellationToken = default(CancellationToken))
		{
			return SaveChangesAsync(cancellationToken);
		}

		public Task<EntityEntry<TEntityModel>> AddAsync<TEntityModel, TId>(
			TEntityModel model,
			CancellationToken cancellationToken = default(CancellationToken))
			where TEntityModel : class, IEntityModel<TId> 
			where TId : struct
			=> base.AddAsync(model, cancellationToken);

		public IQueryable<TEntityModel> BuildQuery<TEntityModel>()
			where TEntityModel : class, IEntityModel
			=> Set<TEntityModel>();

		public EntityEntry Update<TEntityModel, TId>(TEntityModel modelToUpdate)
			where TEntityModel : class, IEntityModel<TId>
			where TId : struct
			=> Update(modelToUpdate);

		public EntityEntry Remove<TEntityModel, TId>(TEntityModel modelToDelete)
			where TEntityModel : class, IEntityModel<TId> where TId : struct
			=> Remove(modelToDelete);
	}
}
