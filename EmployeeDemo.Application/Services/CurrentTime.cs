using EmployeeDemo.Application.Interfaces;

namespace EmployeeDemo.Application.Services
{
    public class CurrentTime : ICurrentTime
    {
        public DateTime GetCurrentTime() => DateTime.UtcNow;
    }
}
