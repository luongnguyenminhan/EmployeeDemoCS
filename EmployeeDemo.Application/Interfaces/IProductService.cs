using EmployeeDemo.Application.Commons;
using EmployeeDemo.Application.ViewModels.ProductViewModels;
using EmployeeDemo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Interfaces
{
    public interface IProductService
    {
        public Task<Product> GetProductAsync(Guid productId);

        public Task<Pagination<Product>> GetProductPaginationAsync(PaginationParameter paginationParameter);

        public Task<Product> CreateProductAsync(CreateProductViewModel product);
    }
}
