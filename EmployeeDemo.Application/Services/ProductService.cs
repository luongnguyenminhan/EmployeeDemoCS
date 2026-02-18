using EmployeeDemo.Application.Commons;
using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.ViewModels.ProductViewModels;
using AutoMapper;
using EmployeeDemo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Product> CreateProductAsync(CreateProductViewModel product)
        {
            var productObj = _mapper.Map<Product>(product);
            await _unitOfWork.ProductRepository.AddAsync(productObj);
            var isSuccess = await _unitOfWork.SaveChangeAsync() > 0;
            if (isSuccess)
            {
                return productObj;
            }
            return null;
        }

        public async Task<Product> GetProductAsync(Guid productId)
        {
            return await _unitOfWork.ProductRepository.GetByIdAsync(productId);
        }

        public async Task<Pagination<Product>> GetProductPaginationAsync(PaginationParameter paginationParameter)
        {
            return await _unitOfWork.ProductRepository.ToPagination(paginationParameter);
        }
    }
}
