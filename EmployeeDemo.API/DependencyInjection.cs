using EmployeeDemo.Application.Interfaces;
using System.Diagnostics;
using EmployeeDemo.API.Middlewares;
using EmployeeDemo.API.Services;

namespace EmployeeDemo.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebAPIService(this IServiceCollection services)
        {
            services.AddSingleton<GlobalExceptionMiddleware>();
            services.AddSingleton<PerformanceMiddleware>();
            services.AddSingleton<Stopwatch>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
