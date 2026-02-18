using EmployeeDemo.Application.ViewModels.ProductViewModels;
using AutoMapper;
using EmployeeDemo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Mapper
{
    public class MapperConfigProfile : Profile
    {
        public MapperConfigProfile() 
        {
            CreateMap<CreateProductViewModel, Product>();
        }
    }
}
