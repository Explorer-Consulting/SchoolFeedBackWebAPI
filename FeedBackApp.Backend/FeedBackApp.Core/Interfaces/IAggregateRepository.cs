using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IAggregateRepository<TAggregate, TBuilder>
        where TAggregate : class, IAggregateRoot
        where TBuilder : IAggregateBuilder<TAggregate>
    {
        Task ConstructAggregateInstanceAsync(Action<TBuilder> configure);
    }
}
