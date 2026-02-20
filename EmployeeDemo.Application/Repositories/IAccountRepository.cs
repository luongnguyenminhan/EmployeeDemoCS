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
    public interface IAccountRepository
    {
        Task<ResponseLoginModel> GetUserByEmailAndPassword(AccountLoginDTO account);

        Task<ResponseModel> AddAccount(AccountLoginDTO account, RoleEnums role);

        Task<Account?> GetAccountByEmailAsync(string email);

        Task<ResponseLoginModel> RefreshToken(TokenModel token);

        Task LogoutAsync(int userId, string deviceId);

        Task LogoutAllDevicesAsync(int userId);
    }
}
