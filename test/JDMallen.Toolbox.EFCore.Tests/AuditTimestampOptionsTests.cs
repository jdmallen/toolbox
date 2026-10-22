using JDMallen.Toolbox.EFCore.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace JDMallen.Toolbox.EFCore.Tests;

/// <summary>
/// Verifies <see cref="AuditableEntitySaveChangesInterceptor" /> constructed with
/// <see cref="AuditTimestampOptions" />: custom property names on entities that
/// don't implement <c>IEntityModel</c>, a nullable modified timestamp left null on
/// create, and entities without the named properties left alone.
/// </summary>
public class AuditTimestampOptionsTests
{
	private static readonly DateTimeOffset StartInstant = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

	private static readonly AuditTimestampOptions CustomNames = new()
	{
		CreatedPropertyName = nameof(Sprocket.CreatedAtUTC),
		ModifiedPropertyName = nameof(Sprocket.UpdatedAtUTC),
		StampModifiedOnCreate = false,
	};

	private sealed class Sprocket
	{
		public int ID { get; init; }

		public string Name { get; set; } = string.Empty;

		public DateTime CreatedAtUTC { get; init; }

		public DateTime? UpdatedAtUTC { get; init; }
	}

	// No audit properties at all; must save untouched.
	private sealed class Label
	{
		public int ID { get; init; }

		public string Text { get; init; } = string.Empty;
	}

	private sealed class SprocketContext(DbContextOptions<SprocketContext> options)
		: DbContext(options)
	{
		public DbSet<Sprocket> Sprockets => Set<Sprocket>();

		public DbSet<Label> Labels => Set<Label>();
	}

	private static SprocketContext CreateContext(AuditTimestampOptions options, FakeTimeProvider clock)
		=> new(
			new DbContextOptionsBuilder<SprocketContext>()
				.UseInMemoryDatabase($"sprockets-{Guid.NewGuid()}")
				.AddInterceptors(new AuditableEntitySaveChangesInterceptor(options, clock))
				.Options);

	[Fact]
	public async Task AddedEntity_StampsCreatedAndLeavesNullableModifiedNull()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using SprocketContext context = CreateContext(CustomNames, clock);

		var sprocket = new Sprocket { Name = "Cog" };
		context.Sprockets.Add(sprocket);
		await context.SaveChangesAsync();

		Assert.Equal(StartInstant.UtcDateTime, sprocket.CreatedAtUTC);
		Assert.Null(sprocket.UpdatedAtUTC);
	}

	[Fact]
	public async Task ModifiedEntity_StampsModifiedAndPreservesCreated()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using SprocketContext context = CreateContext(CustomNames, clock);

		var sprocket = new Sprocket { Name = "Cog" };
		context.Sprockets.Add(sprocket);
		await context.SaveChangesAsync();

		clock.Advance(TimeSpan.FromHours(1));
		sprocket.Name = "Cog Mk II";
		await context.SaveChangesAsync();

		Assert.Equal(StartInstant.UtcDateTime, sprocket.CreatedAtUTC);
		Assert.Equal(StartInstant.UtcDateTime.AddHours(1), sprocket.UpdatedAtUTC);
	}

	[Fact]
	public async Task StampModifiedOnCreate_StampsBothOnAdd()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using SprocketContext context = CreateContext(
			CustomNames with { StampModifiedOnCreate = true },
			clock);

		var sprocket = new Sprocket { Name = "Cog" };
		context.Sprockets.Add(sprocket);
		await context.SaveChangesAsync();

		Assert.Equal(StartInstant.UtcDateTime, sprocket.UpdatedAtUTC);
	}

	[Fact]
	public async Task EntityWithoutNamedProperties_SavesUntouched()
	{
		var clock = new FakeTimeProvider(StartInstant);
		await using SprocketContext context = CreateContext(CustomNames, clock);

		context.Labels.Add(new Label { Text = "Front" });

		Assert.Equal(1, await context.SaveChangesAsync());
	}

	[Fact]
	public void Constructor_RejectsBlankPropertyNames()
	{
		Assert.Throws<ArgumentException>(
			() => new AuditableEntitySaveChangesInterceptor(
				CustomNames with { CreatedPropertyName = " " }));
	}
}
