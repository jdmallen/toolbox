using JDMallen.Toolbox.Data.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
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
/// </remarks>
public sealed class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
	private readonly TimeProvider timeProvider;

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
		this.timeProvider = timeProvider ?? TimeProvider.System;
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
		var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

		foreach (var entry in context.ChangeTracker.Entries<IEntityModel>())
		{
			// IEntityModel exposes the timestamps as read-only, so the change
			// tracker's metadata is used to set them rather than the CLR setters.
			switch (entry.State)
			{
				case EntityState.Added:
					entry.Property(nameof(IEntityModel.DateCreated)).CurrentValue =
						nowUtc;
					entry.Property(nameof(IEntityModel.DateModified)).CurrentValue =
						nowUtc;
					break;
				case EntityState.Modified:
					entry.Property(nameof(IEntityModel.DateModified)).CurrentValue =
						nowUtc;
					break;
			}
		}
	}
}
