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
