using System;
using System.Data;
using System.Linq;
using JDMallen.Toolbox.EFCore.Models;
using JDMallen.Toolbox.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JDMallen.Toolbox.EFCore.Config
{
	/// <inheritdoc cref="DbContext" />
	/// <summary>
	/// Custom abstract extension of <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> that allows for the 
	/// <see cref="T:Microsoft.EntityFrameworkCore.ModelBuilder" /> code to live inside the <see cref="T:JDMallen.Toolbox.EFCore.Models.IComplexEntityModel" />s 
	/// themselves, rather than in one massive method here.
	/// </summary>
	public abstract class EFContextBase : DbContext, IContext
	{
		/// <inheritdoc />
		/// <param name="options"></param>
		/// <param name="enableEntityLevelOnModelCreating">
		/// Set this to false if you plan on doing all your OnModelCreating
		/// Fluent API code from within the DbContext class itself, rather
		/// than in the individual entities.
		/// </param>
		protected EFContextBase(
			DbContextOptions options,
			bool enableEntityLevelOnModelCreating = true) : base(options)
		{
			EnableEntityLevelOnModelCreating = enableEntityLevelOnModelCreating;
		}

		protected bool EnableEntityLevelOnModelCreating { get; set; }

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
			if (!EnableEntityLevelOnModelCreating)
			{
				base.OnModelCreating(modelBuilder);
				return;
			}

			GetType()
				.Assembly
				.DefinedTypes
				// Get all the types that implement
				.Where(
					type => typeof(IEntityModel).IsAssignableFrom(type)
					        // IEntityModel and are concrete classes
					        && !type.IsAbstract
					        && !type.IsInterface)
				.ToList() // Compile them all into a list
				.ForEach(
					type =>
					{
						// And add each one to the ModelBuilder.
						if (modelBuilder.Model.FindEntityType(type) != null)
							return;
						// If it's already been added, skip it.
						modelBuilder.Model.AddEntityType(type);
					});

			GetType()
				.Assembly
				.DefinedTypes
				// Get all the types that implement
				.Where(
					type => typeof(IComplexEntityModel).IsAssignableFrom(type)
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

		public IDbConnection GetConnection() => base.Database.GetDbConnection();
	}
}
