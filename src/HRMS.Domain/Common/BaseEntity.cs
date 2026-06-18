namespace HRMS.Domain.Common;

/// <summary>
/// Abstract base entity providing identity, tenant scoping, audit fields, and domain event support.
/// All domain entities inherit from this class.
/// </summary>
public abstract class BaseEntity<TId> where TId : notnull
{
    public TId Id { get; protected init; } = default!;
    public Guid TenantId { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
    }

    protected BaseEntity(TId id, Guid tenantId)
    {
        Id = id;
        TenantId = tenantId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
