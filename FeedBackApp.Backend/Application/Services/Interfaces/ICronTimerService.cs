
namespace Application.Services.Interfaces
{
    public interface ICronTimerService : IDisposable
    {
        void Start();

        void Stop();
    }
}
