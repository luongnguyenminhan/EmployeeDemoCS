using EmployeeDemo.Application.Commons;
using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class MeetingRepository : GenericRepository<Meeting>, IMeetingRepository
    {
        private readonly AppDbContext _context;

        public MeetingRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService)
            : base(dbContext, timeService, claimsService)
        {
            _context = dbContext;
        }

        public async Task<Pagination<Meeting>> GetUserMeetingPaginationAsync(int userId, PaginationParameter paginationParameter)
        {
            // host or participant (join through MeetingParticipants)
            var query = _context.Meetings
                                .Where(m => m.HostId == userId ||
                                            m.Participants.Any(mp => mp.AccountId == userId))
                                .OrderByDescending(m => m.CreationDate);

            var count = await query.CountAsync();
            var items = await query.Skip((paginationParameter.PageIndex - 1) * paginationParameter.PageSize)
                                   .Take(paginationParameter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();
            return new Pagination<Meeting>(items, count, paginationParameter.PageIndex, paginationParameter.PageSize);
        }

        public Task<Meeting?> GetByIdWithParticipantsAsync(int id)
        {
            return _context.Meetings
                           .Include(m => m.Participants)
                           .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
