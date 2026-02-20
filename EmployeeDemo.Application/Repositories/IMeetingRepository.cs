using EmployeeDemo.Domain.Entities;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Repositories
{
    public interface IMeetingRepository
    {
        Task<Meeting> AddMeetingAsync(Meeting meeting);
    }
}
