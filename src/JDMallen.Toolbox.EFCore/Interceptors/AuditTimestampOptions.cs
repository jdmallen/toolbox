using JDMallen.Toolbox.Data.Abstractions.Interfaces;

namespace JDMallen.Toolbox.EFCore.Interceptors;

/// <summary>
/// Names the audit timestamp properties that
/// <see cref="AuditableEntitySaveChangesInterceptor" /> stamps, for entities that
/// don't use the <see cref="IEntityModel" /> names (for example
/// <c>CreatedAtUTC</c> / <c>UpdatedAtUTC</c>).
/// </summary>
/// <remarks>
/// When the interceptor is constructed with options, it stamps <em>any</em>
/// tracked entity whose EF Core model has the named property, whether or not it
/// implements <see cref="IEntityModel" />. Entities lacking a named property are
/// skipped for that property. Both <see cref="DateTime" /> and nullable
/// <see cref="DateTime" /> properties are supported.
/// </remarks>
public sealed record AuditTimestampOptions
{
	/// <summary>
	/// The property set to the current UTC time when an entity is added.
	/// Defaults to <c>DateCreated</c>.
	/// </summary>
	public string CreatedPropertyName { get; init; } = nameof(IEntityModel.DateCreated);

	/// <summary>
	/// The property set to the current UTC time when an entity is modified (and,
	/// unless <see cref="StampModifiedOnCreate" /> is false, when it is added).
	/// Defaults to <c>DateModified</c>.
	/// </summary>
	public string ModifiedPropertyName { get; init; } = nameof(IEntityModel.DateModified);

	/// <summary>
	/// Whether an added entity also gets its modified timestamp. Defaults to
	/// <see langword="true" />, matching <see cref="IEntityModel" />'s non-nullable
	/// <c>DateModified</c>. Set to <see langword="false" /> for a nullable
	/// "updated at" that should stay null until the first update.
	/// </summary>
	public bool StampModifiedOnCreate { get; init; } = true;
}
