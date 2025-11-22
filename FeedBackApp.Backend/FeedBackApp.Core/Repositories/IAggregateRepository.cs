using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULID = NUlid.Ulid;

namespace Core.Repositories
{
    public interface IAggregateRepository<TAggregate> where TAggregate : IAggregateRoot
    {
        Task CreateAsync(TAggregate aggregate);
        Task RemoveAsync(TAggregate aggregate);
        Task UpdateAsync(TAggregate oldAggregate, TAggregate newAggregate);
        Task<TAggregate> RetrieveAsync(ULID aggregateId);
        Task<IAsyncEnumerable<TAggregate>> RetrieveAllAsync();
    }
}
