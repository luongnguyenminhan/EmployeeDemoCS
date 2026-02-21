using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace EmployeeDemo.Application.Services
{
    /// <summary>
    /// Business layer operations related to accounts.  All pure database interactions
    /// are delegated to <see cref="IAccountRepository"/>, while this class is
    /// for orchestration, validation and token management.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICurrentTime _timeService;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IClaimsService _claimsService;
        private readonly RedisTokenStore _tokenStore;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(
            IAccountRepository accountRepository,
            ICurrentTime timeService,
            IPasswordHasher<Account> passwordHasher,
            IConfiguration configuration,
            IClaimsService claimsService,
            RedisTokenStore tokenStore,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _timeService = timeService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _claimsService = claimsService;
            _tokenStore = tokenStore;
            _unitOfWork = unitOfWork;
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            // simple passthrough, repository handles the query
            return await _accountRepository.GetAccountByEmailAsync(email);
        }

        public async Task<ResponseLoginModel> LoginAsync(AccountLoginDTO account, DeviceMetadata deviceMetadata)
        {
            var accountEntity = await _accountRepository.GetAccountWithRolesByEmailAsync(account.Email);
            if (accountEntity == null)
                return new ResponseLoginModel { Status = false, Message = "Incorrect email or password" };

            var verify = _passwordHasher.VerifyHashedPassword(accountEntity, accountEntity.PasswordHash, account.Password);
            if (verify != PasswordVerificationResult.Success)
                return new ResponseLoginModel { Status = false, Message = "Incorrect email or password" };

            var roles = accountEntity.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? new List<string>();

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, accountEntity.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var r in roles) authClaims.Add(new Claim(ClaimTypes.Role, r));

            var deviceId = DeviceFingerprintService.GenerateDeviceId(deviceMetadata);

            var existingSession = await _tokenStore.GetSessionAsync(accountEntity.Id, deviceId);

            var jwtToken = GenerateJWTToken.CreateToken(authClaims, _configuration, _timeService.GetCurrentTime(), deviceId);
            var jwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = TokenTools.GenerateRefreshToken();

            var hashKey = _configuration["JWT:RefreshTokenHashKey"]!;
            var hashedRefreshToken = RedisTokenStore.HashRefreshToken(refreshToken, hashKey);
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            int expiryInSeconds = (refreshTokenValidityInDays > 0 ? refreshTokenValidityInDays : 7) * 86400;

            try
            {
            await _tokenStore.StoreAsync(accountEntity.Id, deviceId, hashedRefreshToken, expiryInSeconds);

                if (existingSession == null && deviceMetadata != null)
                {
                    var session = new SessionModel
                    {
                        DeviceId = deviceId,
                        DeviceName = "",
                        IpAddress = deviceMetadata.IpAddress,
                        UserAgent = deviceMetadata.UserAgent,
                        OS = "",
                        Browser = "",
                        CreatedAt = DateTime.UtcNow,
                        LastActivity = DateTime.UtcNow
                    };
                    await _tokenStore.StoreSessionAsync(accountEntity.Id, deviceId, session, expiryInSeconds);
                }
            }
            catch (Exception ex)
            {
                return new ResponseLoginModel { Status = false, Message = $"Token storage failed: {ex.Message}" };
            }

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Login successfully",
                JWT = jwt,
                Expired = DateTime.UtcNow.AddMinutes(int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out var m) ? m : 0),
                JWTRefreshToken = refreshToken,
                DeviceId = deviceId
            };
        }

        public async Task<ResponseLoginModel> RefreshToken(TokenModel token)
        {
            if (token is null)
            {
                return new ResponseLoginModel { Status = false, Message = "Token is null" };
            }

            var principal = TokenTools.GetPrincipalFromExpiredToken(token.AccessToken, _configuration);
            if (principal == null)
                return new ResponseLoginModel { Status = false, Message = "Invalid access token or refresh token!" };

            if (!int.TryParse(principal.Identity?.Name, out var accountId))
                return new ResponseLoginModel { Status = false, Message = "Invalid account id in token." };

            var identity = principal.Identity as ClaimsIdentity;
            var deviceId = AuthenTools.GetDeviceIdFromClaims(identity);
            if (string.IsNullOrWhiteSpace(deviceId))
                return new ResponseLoginModel { Status = false, Message = "Device ID not found in token. Token may be malformed or expired." };

            var storedHash = await _tokenStore.GetAsync(accountId, deviceId);
            if (storedHash == null)
                return new ResponseLoginModel { Status = false, Message = "Invalid access token or refresh token!" };

            var hashKey2 = _configuration["JWT:RefreshTokenHashKey"]!;
            if (!RedisTokenStore.VerifyRefreshToken(token.RefreshToken, storedHash, hashKey2))
                return new ResponseLoginModel { Status = false, Message = "Invalid access token or refresh token!" };

            var newAccessTokenToken = GenerateJWTToken.CreateToken(principal.Claims.ToList(), _configuration, _timeService.GetCurrentTime(), deviceId);
            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessTokenToken);
            var newRefreshToken = TokenTools.GenerateRefreshToken();
            var hashKey3 = _configuration["JWT:RefreshTokenHashKey"]!;
            var hashedNewRefreshToken = RedisTokenStore.HashRefreshToken(newRefreshToken, hashKey3);
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            int expiryInSeconds = (refreshTokenValidityInDays > 0 ? refreshTokenValidityInDays : 7) * 86400;

            try
            {
                await _tokenStore.StoreAsync(accountId, deviceId, hashedNewRefreshToken, expiryInSeconds);
                await _tokenStore.UpdateLastActivityAsync(accountId, deviceId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                return new ResponseLoginModel { Status = false, Message = $"Token storage failed: {ex.Message}" };
            }

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Refresh Token successfully!",
                JWT = newAccessToken,
                Expired = DateTime.UtcNow.AddMinutes(int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out var mm) ? mm : 0),
                JWTRefreshToken = newRefreshToken,
                DeviceId = deviceId
            };
        }

        public async Task<ResponseModel> ResigerAsync(AccountLoginDTO account, RoleEnums role)
        {
            var existing = await _accountRepository.GetAccountByEmailAsync(account.Email);
            if (existing != null)
                return new ResponseModel { Status = false, Message = "Username already exists" };

            var domainAccount = new Account
            {
                Email = account.Email,
                UserName = account.Email,
                CreatedBy = _claimsService.GetCurrentUserId,
                CreationDate = _timeService.GetCurrentTime(),
                SecurityStamp = Guid.NewGuid().ToString()
            };
            domainAccount.PasswordHash = _passwordHasher.HashPassword(domainAccount, account.Password);

            await _accountRepository.AddAsync(domainAccount);

            // assign requested role
            var roleName = role.ToString();
            var roleEntity = await _accountRepository.GetRoleByNameAsync(roleName);
            if (roleEntity == null)
            {
                roleEntity = new Role { Name = roleName };
                await _accountRepository.AddRoleAsync(roleEntity);
            }

            // ensure USER role exists and assign as default
            var userRole = await _accountRepository.GetRoleByNameAsync(RoleEnums.USER.ToString());
            if (userRole == null)
            {
                userRole = new Role { Name = RoleEnums.USER.ToString() };
                await _accountRepository.AddRoleAsync(userRole);
            }

            // add account roles (use navigation to keep fks correct after save)
            await _accountRepository.AddAccountRoleAsync(new AccountRole
            {
                Account = domainAccount,
                Role = roleEntity
            });

            // avoid duplicate if requested role is same as default USER
            if (roleEntity.Id != userRole.Id)
            {
                await _accountRepository.AddAccountRoleAsync(new AccountRole
                {
                    Account = domainAccount,
                    Role = userRole
                });
            }

            // commit everything in one transaction
            await _unitOfWork.SaveChangeAsync();

            return new ResponseModel { Status = true, Message = "Your account is ready. Try to login now." };
        }

        public async Task LogoutAsync(int userId, string deviceId)
        {
            await _tokenStore.RemoveAsync(userId, deviceId);
        }

        public async Task LogoutAllDevicesAsync(int userId)
        {
            await _tokenStore.RemoveAllAsync(userId);
        }

        public async Task<CurrentUserSessionResponse?> GetCurrentUserWithSessionsAsync(int userId, string currentDeviceId)
        {
            var account = await _accountRepository.GetAccountWithRolesByIdAsync(userId);
            if (account == null)
                return null;

            var roles = account.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? new List<string>();
            var userInfo = new UserSessionInfo(account.Id, account.Email ?? "", account.UserName ?? "", roles.AsReadOnly());

            var currentSession = await _tokenStore.GetSessionAsync(userId, currentDeviceId);
            var allSessions = await _tokenStore.GetAllSessionsAsync(userId);

            return new CurrentUserSessionResponse(userInfo, currentSession, allSessions.AsReadOnly());
        }
    }
}
