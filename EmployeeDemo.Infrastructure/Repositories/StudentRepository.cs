using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICurrentTime _timeService;

        public StudentRepository(AppDbContext context,
            ICurrentTime timeService,
            IClaimsService claimsService, IConfiguration configuration)
            : base(context, timeService, claimsService)
        {
            _context = context;
            _configuration = configuration;
            _timeService = timeService;
        }

        public async Task<ResponseLoginModel> LoginStudentAsync(string email)
        {
            var student = await _context.Students.SingleOrDefaultAsync(s => s.StudentEmail == email);

            if (student == null)
            {
                return new ResponseLoginModel { Status = false, Message = "Not found an account" };
            }
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, student.Id.ToString()),
                new Claim(ClaimTypes.Role, RoleEnums.STUDENT.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //generate refresh token
            var refreshToken = TokenTools.GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            student.RefreshToken = refreshToken;
            student.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            _context.Update(student);
            await _context.SaveChangesAsync();

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

        public async Task<ResponseLoginModel> RefreshTokenStudent(TokenModel token)
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

            Guid studentId = Guid.Parse(principal.Identity.Name);

            var student = await _context.Students.FirstOrDefaultAsync(a => a.Id == studentId);

            if (student == null || student.RefreshToken != refreshToken || student.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            var newAccessToken = GenerateJWTToken.CreateToken(principal.Claims.ToList(), _configuration, _timeService.GetCurrentTime());
            var newRefreshToken = TokenTools.GenerateRefreshToken();

            student.RefreshToken = newRefreshToken;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();

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
