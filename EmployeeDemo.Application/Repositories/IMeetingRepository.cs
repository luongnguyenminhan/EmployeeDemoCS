using EmployeeDemo.Application.Commons;
using EmployeeDemo.Domain.Entities;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Repositories
{
    public interface IMeetingRepository: IGenericRepository<Meeting>
    {
        // inherits AddAsync from generic; no extra methods required

        // returns paged meetings where the given user is host or participant
        Task<Pagination<Meeting>> GetUserMeetingPaginationAsync(int userId, PaginationParameter paginationParameter);

        // load meeting with participants included, or null if not found
        Task<Meeting?> GetByIdWithParticipantsAsync(int id);
    }
}
