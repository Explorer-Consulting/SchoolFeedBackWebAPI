namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        // these methods depends on performance/customer needs and can be changed in the future
        Task Deliver(string EmailAddress);
        Task Deliver(/*implementation-dependent*/);
    }
}
