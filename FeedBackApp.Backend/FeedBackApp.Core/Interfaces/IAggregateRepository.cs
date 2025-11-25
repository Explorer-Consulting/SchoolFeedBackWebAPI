using System.Linq.Expressions;

namespace Core.Interfaces;

public interface IAggregateRepository<TAggregate, TBuilder>
    where TAggregate : class, IAggregateRoot
    where TBuilder : IAggregateBuilder<TAggregate>
{
    /// <summary>
    /// Asynchronously constructs a new aggregate instance using the specified builder configuration.
    /// </summary>
    /// <param name="configure">A delegate that configures the aggregate builder before the instance is created. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation of constructing the aggregate instance.</returns>
    Task ConstructAggregateInstanceAsync(Action<TBuilder> configure);

    /// <summary>
    /// Deletes the aggregate with the specified identifier asynchronously.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate to delete. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteAggregateAsync(string aggregateId);

    /// <summary>
    /// Asynchronously updates the aggregate identified by the specified ID using the provided configuration action.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate to update. Cannot be null or empty.</param>
    /// <param name="configure">An action that configures the aggregate builder before the update is applied.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateAggregateAsync(string aggregateId, Action<TBuilder> configure);

    /// <summary>
    /// Asynchronously retrieves a single aggregate entity that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression that defines the criteria used to select the aggregate entity. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the aggregate entity that matches
    /// the predicate, or null if no matching entity is found.</returns>
    Task<TAggregate?> RetrieveAggregateAsync(Expression<Func<TAggregate, bool>> predicate);

    /// <summary>
    /// Returns an asynchronous sequence of all aggregate entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">An optional expression used to filter the aggregates. If null, all aggregates are returned.</param>
    /// <returns>An asynchronous sequence of aggregate entities of type TAggregate that satisfy the predicate. The sequence is
    /// empty if no aggregates match.</returns>
    IAsyncEnumerable<TAggregate> RetrieveAllAggregatesAsync(Expression<Func<TAggregate, bool>>? predicate = null);
}