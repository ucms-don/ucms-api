namespace Ucms.Domain.Common;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Базовый класс для сущностей
/// </summary>
public abstract class Entity : IEntity
{
    [Key]
    public Guid Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
