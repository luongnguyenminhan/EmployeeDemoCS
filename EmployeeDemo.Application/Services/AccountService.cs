using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
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

namespace EmployeeDemo.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository) 
        {
            _accountRepository = accountRepository;
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _accountRepository.GetAccountByEmailAsync(email);
        }

        public async Task<ResponseLoginModel> LoginAsync(AccountLoginDTO account)
        {
            return await _accountRepository.GetUserByEmailAndPassword(account);
        }

        public async Task<ResponseLoginModel> RefreshToken(TokenModel token)
        {
            return await _accountRepository.RefreshToken(token);
        }

        public async Task<ResponseModel> ResigerAsync(AccountLoginDTO account, RoleEnums role)
        {
            return await _accountRepository.AddAccount(account, role);
        }

        public async Task LogoutAsync(int userId, string deviceId)
        {
            await _accountRepository.LogoutAsync(userId, deviceId);
        }

        public async Task LogoutAllDevicesAsync(int userId)
        {
            await _accountRepository.LogoutAllDevicesAsync(userId);
        }
    }
}
