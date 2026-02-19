using EmployeeDemo.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application
{
    public interface IUnitOfWork
    {
        // ProductRepository removed — Product entity/services deleted

        public Task<int> SaveChangeAsync();
    }
}
