using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Domain.Entities;
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
    }
}
