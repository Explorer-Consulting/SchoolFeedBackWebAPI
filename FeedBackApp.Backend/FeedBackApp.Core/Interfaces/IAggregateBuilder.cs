namespace Core.Interfaces;

/// <summary>
/// Defines a contract for building aggregate root instances asynchronously.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root to be built. Must implement <see cref="IAggregateRoot"/>.</typeparam>
public interface IAggregateBuilder<TAggregate>
    where TAggregate : IAggregateRoot
{
    Task<TAggregate> BuildAggregateAsync();
}
