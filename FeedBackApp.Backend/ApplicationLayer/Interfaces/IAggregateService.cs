using Core.Interfaces;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ApplicationLayer.Interfaces;

public interface IAggregateService<TAggregateDTO, TAggregate>
    where TAggregateDTO : class, IAggregateDTORoot
    where TAggregate : class, IAggregateRoot
{
    Task<Result> ConstructAggreateInstanceAsync(TAggregateDTO dto);
    Task<Result> DeleteAggregateAsync(string aggregateId);
    Task<Result> UpdateAggregateAsync(TAggregateDTO dto);
    Task<Result<TAggregateDTO>> RetrieveAggregateAsync(Expression<Func<TAggregate, bool>> predicate);
    Task<Result<IReadOnlyCollection<TAggregateDTO>>> RetrieveAllAggregatesAsync(
        Expression<Func<TAggregate, bool>>? predicate = null);
}
