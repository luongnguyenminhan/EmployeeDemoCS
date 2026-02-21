using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Repositories
{
    public interface IAccountRepository: IGenericRepository<Account>
    {
        // basic profile queries
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<Account?> GetAccountWithRolesByEmailAsync(string email);
        Task<Account?> GetAccountWithRolesByIdAsync(int accountId);

        // role helpers
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task AddRoleAsync(Role role);
        Task AddAccountRoleAsync(AccountRole accountRole);
    }
}
