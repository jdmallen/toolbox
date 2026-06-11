namespace JDMallen.Toolbox.Data.Abstractions.Interfaces;

/// <summary>
/// Marker interface for aggregate root entities in domain-driven design.
/// Aggregate roots are the entry points for all operations on an aggregate.
/// </summary>
public interface IAggregateRoot : IEntityModel;
