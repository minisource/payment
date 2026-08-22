using Minisource.Common.Domain;

namespace Domain.Abstractions;

/// <summary>
/// Dispatches domain events to in-process handlers.
/// Implementations live in the Application layer.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct);
}
