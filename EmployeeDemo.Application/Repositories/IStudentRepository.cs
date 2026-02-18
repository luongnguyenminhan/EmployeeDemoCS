using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Repositories
{
    public interface IStudentRepository : IGenericRepository<Student>
    {
        public Task<ResponseLoginModel> LoginStudentAsync(string email);

        public Task<ResponseLoginModel> RefreshTokenStudent(TokenModel token);
    }
}
