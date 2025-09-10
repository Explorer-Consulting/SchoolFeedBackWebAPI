using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ReportCompilationException : Exception
    {
        public ReportCompilationException() { }

        public ReportCompilationException(string message)
            : base(message) { }

        public ReportCompilationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
