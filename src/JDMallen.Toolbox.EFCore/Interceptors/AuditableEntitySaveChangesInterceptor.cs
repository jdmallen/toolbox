using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JDMallen.Toolbox.EFCore.Interceptors;

/// <summary>
/// A <see cref="SaveChangesInterceptor" /> that stamps the audit timestamps on
/// any tracked entity implementing <see cref="IEntityModel" /> whenever changes
/// are saved. Added entities receive both
/// <see cref="IEntityModel.DateCreated" /> and
/// <see cref="IEntityModel.DateModified" />; modified entities receive a fresh
/// <see cref="IEntityModel.DateModified" />.
/// </summary>
/// <remarks>
/// This realizes the audit half of the <see cref="IEntityModel" /> convention so
/// the timestamps are maintained centrally rather than by every call site. Pair
/// it with <see cref="Extensions.SoftDeleteModelBuilderExtensions" /> for the
/// soft-delete half.
/// <para>
/// To stamp entities that use other property names, construct it with
/// <see cref="AuditTimestampOptions" />; see that type for the matching rules.
/// </para>
/// </remarks>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
	private static readonly AuditTimestampOptions DefaultOptions = new();

	private readonly TimeProvider _timeProvider;

	// Null keeps the original behavior: IEntityModel entities only, with the
	// IEntityModel property names.
	private readonly AuditTimestampOptions? _options;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="AuditableEntitySaveChangesInterceptor" /> class.
	/// </summary>
	/// <param name="timeProvider">
	/// The clock used to source the audit timestamps. Defaults to
	/// <see cref="TimeProvider.System" /> when not supplied; pass a fake clock to
	/// make stamping deterministic in tests.
	/// </param>
	public AuditableEntitySaveChangesInterceptor(TimeProvider? timeProvider = null)
	{
		_timeProvider = timeProvider ?? TimeProvider.System;
	}

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="AuditableEntitySaveChangesInterceptor" /> class that stamps the
	/// properties named in <paramref name="options" /> on any entity that has
	/// them, not only on <see cref="IEntityModel" /> implementations.
	/// </summary>
	/// <param name="options">The audit property names to stamp.</param>
	/// <param name="timeProvider">
	/// The clock used to source the audit timestamps. Defaults to
	/// <see cref="TimeProvider.System" /> when not supplied.
	/// </param>
	public AuditableEntitySaveChangesInterceptor(
		AuditTimestampOptions options,
		TimeProvider? timeProvider = null)
		: this(timeProvider)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentException.ThrowIfNullOrWhiteSpace(options.CreatedPropertyName);
		ArgumentException.ThrowIfNullOrWhiteSpace(options.ModifiedPropertyName);

		_options = options;
	}

	/// <inheritdoc />
	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData,
		InterceptionResult<int> result)
	{
		StampAuditTimestamps(eventData.Context);

		return base.SavingChanges(eventData, result);
	}

	/// <inheritdoc />
	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		StampAuditTimestamps(eventData.Context);

		return base.SavingChangesAsync(eventData, result, cancellationToken);
	}

	private void StampAuditTimestamps(DbContext? context)
	{
		if (context is null)
		{
			return;
		}

		// UtcDateTime keeps the stored value clock-agnostic regardless of the
		// supplied TimeProvider's local offset.
		DateTime nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

		AuditTimestampOptions options = _options ?? DefaultOptions;

		IEnumerable<EntityEntry> entries = _options is null
			? context.ChangeTracker.Entries<IEntityModel>()
			: context.ChangeTracker.Entries();

		foreach (EntityEntry entry in entries)
		{
			// IEntityModel exposes the timestamps as read-only, so the change
			// tracker's metadata is used to set them rather than the CLR setters.
			switch (entry.State)
			{
				case EntityState.Added:
					StampIfMapped(entry, options.CreatedPropertyName, nowUtc);

					if (options.StampModifiedOnCreate)
					{
						StampIfMapped(entry, options.ModifiedPropertyName, nowUtc);
					}

					break;
				case EntityState.Modified:
					StampIfMapped(entry, options.ModifiedPropertyName, nowUtc);

					break;
				case EntityState.Detached:
				case EntityState.Unchanged:
				case EntityState.Deleted:
					// No-op.
					break;
				default:
#pragma warning disable CA2208 // Roslyn's angry because it's not a method param
					throw new ArgumentOutOfRangeException(
#pragma warning restore CA2208
						nameof(entry.State),
						entry.State,
						"Unexpected entity state encountered when stamping audit timestamps.");
			}
		}
	}

	// With custom options, entities that lack a named property are skipped for
	// it; IEntityModel entities always have the default names.
	private static void StampIfMapped(EntityEntry entry, string propertyName, DateTime nowUtc)
	{
		if (entry.Metadata.FindProperty(propertyName) is not null)
		{
			entry.Property(propertyName).CurrentValue = nowUtc;
		}
	}
}
