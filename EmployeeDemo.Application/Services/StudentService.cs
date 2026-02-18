using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository) 
        { 
            _studentRepository = studentRepository;
        }
        public async Task<ResponseLoginModel> LoginWithEmailAsync(string email)
        {
            return await _studentRepository.LoginStudentAsync(email);
        }

        public async Task<ResponseLoginModel> RefreshToken(TokenModel token)
        {
            return await _studentRepository.RefreshTokenStudent(token);
        }
    }
}
