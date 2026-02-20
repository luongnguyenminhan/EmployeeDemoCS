using EmployeeDemo.Application.Repositories;

namespace EmployeeDemo.Application.Interfaces
{
    public interface IClaimsService
    {
        public int GetCurrentUserId { get; }

        public string GetDeviceId { get; }
    }
}
