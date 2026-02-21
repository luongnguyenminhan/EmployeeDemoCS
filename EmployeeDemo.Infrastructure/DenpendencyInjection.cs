using EmployeeDemo.Application;
using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Services;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Infrastructure.Mapper;
using EmployeeDemo.Infrastructure.Repositories;
using EmployeeDemo.Infrastructure.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace EmployeeDemo.Infrastructure
{
    public static class DenpendencyInjection
    {
        public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, string databaseConnection, IConfiguration configuration)
        {
            // we no longer use ASP.NET Identity types — register password hasher for Account
            services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

            services.AddDbContext<AppDbContext>(option => 
                option.UseMySql(databaseConnection, ServerVersion.AutoDetect(databaseConnection)));

            // Redis configuration and connection
            var redisSettings = new RedisSettings();
            configuration.GetSection("Redis").Bind(redisSettings);
            services.AddSingleton(redisSettings);

            // Register Redis ConnectionMultiplexer as singleton (thread-safe, reusable)
            // Use ConfigurationOptions and set AbortOnConnectFail = false so the app won't fail startup when Redis is temporarily unavailable.
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<RedisSettings>();
                var options = ConfigurationOptions.Parse(settings.ConnectionString);
                options.AbortOnConnectFail = false;   // continue retrying instead of aborting the multiplexer
                options.ConnectRetry = 5;            // retry attempts for transient failures

                return ConnectionMultiplexer.Connect(options);
            });

            // register redis token store concrete (used directly by application)
            services.AddScoped<RedisTokenStore>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

            // the helper classes have been converted to static utils; no DI registration needed

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<ICurrentTime, CurrentTime>();

            services.AddAutoMapper(typeof(MapperConfigProfile).Assembly);

            return services;
        }
    }
}
