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

namespace EmployeeDemo.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ResponseLoginModel> LoginAsync(AccountLoginDTO account, ViewModels.DeviceMetadata deviceMetadata);

        Task<ResponseModel> ResigerAsync(AccountLoginDTO account, RoleEnums role);

        Task<Account?> GetAccountByEmailAsync(string email);

        Task<ResponseLoginModel> RefreshToken(TokenModel token);

        Task LogoutAsync(int userId, string deviceId);

        Task LogoutAllDevicesAsync(int userId);

        Task<CurrentUserSessionResponse?> GetCurrentUserWithSessionsAsync(int userId, string currentDeviceId);
    }
}
