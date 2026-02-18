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
        public IProductRepository ProductRepository { get; }

        public Task<int> SaveChangeAsync();
    }
}
