using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public sealed class ReportStorageException : Exception
    {
        public ReportStorageException(string message, Exception inner) : base(message, inner) { }
        public ReportStorageException(string message) : base(message) { }
    }
}
