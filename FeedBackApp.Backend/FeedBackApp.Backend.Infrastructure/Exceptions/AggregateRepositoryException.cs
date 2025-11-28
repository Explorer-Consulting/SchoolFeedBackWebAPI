using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Exceptions
{
    public sealed class AggregateRepositoryException(string message, Exception? innerException = default!) : Exception(message, innerException)
    {
        public AggregateRepositoryException() : this("An error occurred in the Infrastructure layer.") { }

        public AggregateRepositoryException(Exception innerException) : this("An error occurred in the Infrastructure layer.", innerException) { }
    }
}
