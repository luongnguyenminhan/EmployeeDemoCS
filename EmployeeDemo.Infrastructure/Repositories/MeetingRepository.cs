using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Domain.Entities;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly AppDbContext _dbContext;

        public MeetingRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Meeting> AddMeetingAsync(Meeting meeting)
        {
            _dbContext.Meetings.Add(meeting);
            await _dbContext.SaveChangesAsync();
            return meeting;
        }
    }
}
