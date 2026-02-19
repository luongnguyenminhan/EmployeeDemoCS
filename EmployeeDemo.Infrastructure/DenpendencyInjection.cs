using EmployeeDemo.Application;
using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Services;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Infrastructure.Mapper;
using EmployeeDemo.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace EmployeeDemo.Infrastructure
{
    public static class DenpendencyInjection
    {
        public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, string databaseConnection)
        {

            services.AddIdentity<Account, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddDbContext<AppDbContext>(option => 
                option.UseMySql(databaseConnection, ServerVersion.AutoDetect(databaseConnection)));

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

            // Product and Student repositories/services removed per request
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IProductService, ProductService>();
            // services.AddScoped<IStudentRepository, StudentRepository>();
            // services.AddScoped<IStudentService, StudentService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<ICurrentTime, CurrentTime>();

            services.AddAutoMapper(typeof(MapperConfigProfile).Assembly);


            return services;
        }
    }
}
