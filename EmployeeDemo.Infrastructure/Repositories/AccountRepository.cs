using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentTime _timeService;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IClaimsService _claimsService;

        public AccountRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService,
            IPasswordHasher<Account> passwordHasher,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _claimsService = claimsService;
            _timeService = timeService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<ResponseModel> AddAccount(AccountLoginDTO account, RoleEnums role)
        {
            try
            {
                var existing = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == account.Email);
                if (existing != null)
                    return new ResponseModel { Status = false, Message = "Username already exists" };

                // create domain profile (Account) and hash password
                var domainAccount = new Account
                {
                    Email = account.Email,
                    UserName = account.Email,
                    CreatedBy = _claimsService.GetCurrentUserId,
                    CreationDate = _timeService.GetCurrentTime(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                domainAccount.PasswordHash = _passwordHasher.HashPassword(domainAccount, account.Password);
                _dbContext.Accounts.Add(domainAccount);
                await _dbContext.SaveChangesAsync();



                // ensure requested role exists and assign it to the account (using domain Role / AccountRole)
                var roleName = role.ToString();
                var roleEntity = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (roleEntity == null)
                {
                    roleEntity = new Role { Name = roleName };
                    _dbContext.Roles.Add(roleEntity);
                    await _dbContext.SaveChangesAsync();
                }

                _dbContext.AccountRoles.Add(new AccountRole { AccountId = domainAccount.Id, RoleId = roleEntity.Id });

                // ensure USER role exists and assign as default
                var userRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == RoleEnums.USER.ToString());
                if (userRole == null)
                {
                    userRole = new Role { Name = RoleEnums.USER.ToString() };
                    _dbContext.Roles.Add(userRole);
                    await _dbContext.SaveChangesAsync();
                }
                _dbContext.AccountRoles.Add(new AccountRole { AccountId = domainAccount.Id, RoleId = userRole.Id });

                return new ResponseModel { Status = true, Message = "Your account is ready. Try to login now." };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Message = "Error: " + ex.Message };
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            // return domain Account (profile) by email
            return await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<ResponseLoginModel> GetUserByEmailAndPassword(AccountLoginDTO account)
        {
            var accountEntity = await _dbContext.Accounts
                .Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.Email == account.Email);

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

            // generate refresh token and update domain profile
            var refreshToken = TokenTools.GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            accountEntity.RefreshToken = refreshToken;
            accountEntity.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            _dbContext.Accounts.Update(accountEntity);
            await _dbContext.SaveChangesAsync();

            var token = GenerateJWTToken.CreateToken(authClaims, _configuration, _timeService.GetCurrentTime());

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Login successfully",
                JWT = new JwtSecurityTokenHandler().WriteToken(token),
                Expired = token.ValidTo,
                JWTRefreshToken = refreshToken,
            };
        }

        public async Task<ResponseLoginModel> RefreshToken(TokenModel token)
        {
            if (token is null)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Token is null"
                };
            }

            string? accessToken = token.AccessToken;
            string? refreshToken = token.RefreshToken;

            var principal = TokenTools.GetPrincipalFromExpiredToken(accessToken, _configuration);
            if (principal == null)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            var accountIdStr = principal.Identity?.Name;
            if (!int.TryParse(accountIdStr, out var accountId))
            {
                return new ResponseLoginModel { Status = false, Message = "Invalid account id in token." };
            }

            var account = await _dbContext.Accounts.FindAsync(accountId);
            if (account == null || account.RefreshToken != refreshToken || account.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            var newAccessToken = GenerateJWTToken.CreateToken(principal.Claims.ToList(), _configuration, _timeService.GetCurrentTime());
            var newRefreshToken = TokenTools.GenerateRefreshToken();

            account.RefreshToken = newRefreshToken;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Refresh Token successfully!",
                JWT = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                Expired = newAccessToken.ValidTo,
                JWTRefreshToken = newRefreshToken
            };
        }
    }
}

