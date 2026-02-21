using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService)
            : base(dbContext, timeService, claimsService)
        {
            _context = dbContext;
        }

        // simple query for profile by email
        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        // include roles so service doesn't have to know how to join
        public async Task<Account?> GetAccountWithRolesByEmailAsync(string email)
        {
            return await _context.Accounts
                .Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Account?> GetAccountWithRolesByIdAsync(int accountId)
        {
            return await _context.Accounts
                .Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.Id == accountId && !a.IsDeleted);
        }


        public Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public Task AddRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            return Task.CompletedTask;
        }

        public Task AddAccountRoleAsync(AccountRole accountRole)
        {
            _context.AccountRoles.Add(accountRole);
            return Task.CompletedTask;
        }

    }
}
