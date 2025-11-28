using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IAggregateService<TAggregateDTO> where TAggregateDTO : class, IAggregateDTORoot
    {
        Task ConstructAggreateInstanceAsync(Action<TAggregateDTO> configure);
        Task DeleteAggregateAsync(string aggregateId);
        Task UpdateAggregateAsync(string aggregateId, Action<TAggregateDTO> configure);
        Task<TAggregateDTO?> RetrieveAggregateAsync(Expression<Func<TAggregateDTO, bool>> predicate);
        IAsyncEnumerable<TAggregateDTO> RetrieveAllAggregatesAsync(Expression<Func<TAggregateDTO, bool>>? predicate = null);
    }
}
