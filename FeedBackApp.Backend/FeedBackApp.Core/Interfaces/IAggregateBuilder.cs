namespace Core.Interfaces;

public interface IAggregateBuilder<TAggregate>
    where TAggregate : IAggregateRoot
{
    Task<TAggregate> BuildAggregateAsync();
}
