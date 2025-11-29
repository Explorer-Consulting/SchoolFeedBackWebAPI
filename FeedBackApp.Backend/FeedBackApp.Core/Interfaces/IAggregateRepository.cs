using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IAggregateRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    Task ConstructAggregateInstanceAsync(TAggregate aggregate);

    Task DeleteAggregateAsync(string aggregateId);

    Task UpdateAggregateAsync(TAggregate aggregate);

    Task<TAggregate?> RetrieveAggregateAsync(Expression<Func<TAggregate, bool>> predicate);

    IAsyncEnumerable<TAggregate> RetrieveAllAggregatesAsync(
        Expression<Func<TAggregate, bool>>? predicate = null);
}
