using JDMallen.Toolbox.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
// ReSharper disable ClassNeverInstantiated.Local

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Verifies
/// <see
///   cref="SoftDeleteModelBuilderExtensions.AddSoftDeleteQueryFilter(ModelBuilder, SoftDeleteFilterOptions)" />:
/// a custom property name on entities that don't implement <c>IEntityModel</c>,
/// named filters, shadow properties, and entity types that must be skipped
/// (no property, non-bool property, owned types).
/// </summary>
public class SoftDeleteFilterOptionsTests
{
	private const string SOFT_DELETE = "SoftDelete";

	private sealed class Widget
	{
		public int ID { get; init; }

		public string Name { get; init; } = string.Empty;

		public bool Removed { get; init; }

		public WidgetDetails? Details { get; init; }
	}

	// Owned; carries a bool of the same name, which must not get a filter (EF
	// throws for filters on owned types).
	private sealed class WidgetDetails
	{
		public bool Removed { get; init; }
	}

	// No soft-delete property; must stay unfiltered.
	private sealed class Label
	{
		public int ID { get; init; }

		public string Text { get; init; } = string.Empty;
	}

	// Same name, wrong type; skipped rather than mistranslated.
	private sealed class Counter
	{
		public int ID { get; init; }

		public int Removed { get; init; }
	}

	// Soft-delete flag mapped as a shadow property only.
	private sealed class Gizmo
	{
		public int ID { get; init; }

		public string Name { get; init; } = string.Empty;
	}

	private sealed class WidgetContext(
		DbContextOptions<WidgetContext> options,
		SoftDeleteFilterOptions filterOptions)
		: DbContext(options)
	{
		public SoftDeleteFilterOptions FilterOptions => filterOptions;

		public DbSet<Widget> Widgets => Set<Widget>();

		public DbSet<Gizmo> Gizmos => Set<Gizmo>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Widget>().OwnsOne(widget => widget.Details);

			// Never queried; mapped only so the skip test can inspect them
			modelBuilder.Entity<Label>();
			modelBuilder.Entity<Counter>();

			modelBuilder.Entity<Gizmo>().Property<bool>(nameof(Widget.Removed));

			modelBuilder.AddSoftDeleteQueryFilter(filterOptions);
		}
	}

	private static readonly SoftDeleteFilterOptions RemovedOptions = new()
	{
		PropertyName = nameof(Widget.Removed),
	};

	private static WidgetContext CreateContext(SoftDeleteFilterOptions filterOptions)
		=> new(
			new DbContextOptionsBuilder<WidgetContext>()
				.UseInMemoryDatabase($"widgets-{Guid.NewGuid()}")

				// The model depends on filterOptions, so don't share it between
				// contexts built with different options.
				.ReplaceService<IModelCacheKeyFactory, OptionsModelCacheKeyFactory>()
				.Options,
			filterOptions);

	private sealed class OptionsModelCacheKeyFactory : IModelCacheKeyFactory
	{
		public object Create(DbContext context, bool designTime)
			=> (context.GetType(), ((WidgetContext)context).FilterOptions, designTime);
	}

	private static async Task<WidgetContext> SeededContextAsync(SoftDeleteFilterOptions filterOptions)
	{
		WidgetContext context = CreateContext(filterOptions);

		context.Widgets.Add(new Widget { Name = "Visible" });
		context.Widgets.Add(
			new Widget
			{
				Name = "Hidden",
				Removed = true,
			});
		await context.SaveChangesAsync();

		return context;
	}

	[Fact]
	public async Task CustomPropertyName_ExcludesRemovedEntities()
	{
		await using WidgetContext context = await SeededContextAsync(RemovedOptions);

		List<string> names = await context.Widgets
			.Select(widget => widget.Name)
			.ToListAsync();

		Assert.Equal(["Visible"], names);
	}

	[Fact]
	public async Task NamedFilter_IsIgnoredByName()
	{
		SoftDeleteFilterOptions named = RemovedOptions with { FilterName = SOFT_DELETE };
		await using WidgetContext context = await SeededContextAsync(named);

		List<string> filtered = await context.Widgets
			.Select(widget => widget.Name)
			.ToListAsync();
		List<string> everything = await context.Widgets
			.IgnoreQueryFilters([SOFT_DELETE])
			.Select(widget => widget.Name)
			.OrderBy(name => name)
			.ToListAsync();

		Assert.Equal(["Visible"], filtered);
		Assert.Equal(["Hidden", "Visible"], everything);
	}

	[Fact]
	public async Task ShadowProperty_IsFiltered()
	{
		await using WidgetContext context = CreateContext(RemovedOptions);

		var hidden = new Gizmo { Name = "Hidden" };
		context.Gizmos.Add(new Gizmo { Name = "Visible" });
		context.Gizmos.Add(hidden);
		context.Entry(hidden).Property(nameof(Widget.Removed)).CurrentValue = true;
		await context.SaveChangesAsync();

		List<string> names = await context.Gizmos
			.Select(gizmo => gizmo.Name)
			.ToListAsync();

		Assert.Equal(["Visible"], names);
	}

	[Fact]
	public void EntitiesWithoutABoolProperty_AndOwnedTypes_AreSkipped()
	{
		using WidgetContext context = CreateContext(RemovedOptions);

		Assert.Empty(context.Model.FindEntityType(typeof(Label))!.GetDeclaredQueryFilters());
		Assert.Empty(context.Model.FindEntityType(typeof(Counter))!.GetDeclaredQueryFilters());
		Assert.Empty(context.Model.FindEntityType(typeof(WidgetDetails))!.GetDeclaredQueryFilters());
		Assert.Single(context.Model.FindEntityType(typeof(Widget))!.GetDeclaredQueryFilters());
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void BlankNames_Throw(string blank)
	{
		var modelBuilder = new ModelBuilder();

		Assert.ThrowsAny<ArgumentException>(() => modelBuilder.AddSoftDeleteQueryFilter(
			new SoftDeleteFilterOptions { PropertyName = blank }));
		Assert.ThrowsAny<ArgumentException>(() => modelBuilder.AddSoftDeleteQueryFilter(
			RemovedOptions with { FilterName = blank }));
	}
}
