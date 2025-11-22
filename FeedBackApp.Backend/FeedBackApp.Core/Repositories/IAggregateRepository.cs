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
        Task CreateAggregate(TAggregate aggregate);
        Task RemoveAggregate(TAggregate aggregate);
        Task UpdateAggregate(TAggregate oldAggregate, TAggregate newAggregate);
        Task<TAggregate> RetrieveAggregate(ULID aggregateId);
        Task<ICollection<TAggregate>> RetrieveAll();
    }
}
