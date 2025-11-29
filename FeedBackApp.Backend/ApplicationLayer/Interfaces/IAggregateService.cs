using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ApplicationLayer.Interfaces;

public interface IAggregateService<TAggregateDTO, TAggregate>
    where TAggregateDTO : class, IAggregateDTORoot
    where TAggregate : class, IAggregateRoot
{
    Task ConstructAggreateInstanceAsync(TAggregateDTO dto);

    Task DeleteAggregateAsync(string aggregateId);

    Task UpdateAggregateAsync(TAggregateDTO dto);

    Task<TAggregateDTO?> RetrieveAggregateAsync(
        Expression<Func<TAggregate, bool>> predicate);

    IAsyncEnumerable<TAggregateDTO> RetrieveAllAggregatesAsync(
        Expression<Func<TAggregate, bool>>? predicate = null);
}
