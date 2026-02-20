using EmployeeDemo.API;
using EmployeeDemo.API.Middlewares;
using EmployeeDemo.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add JWT authentication and authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Swagger with JWT security
builder.Services.AddSwaggerJwtSecurity();

// project service registrations (API + Infrastructure)
builder.Services.AddWebAPIService();
#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddInfrastructuresService(builder.Configuration.GetConnectionString("DefaultConnection"), builder.Configuration);
#pragma warning restore CS8604 // Possible null reference argument.

var app = builder.Build();

// Apply pending EF Core migrations at startup (safe: logs errors and rethrows)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // If there are compiled migrations in the project, apply them.
        // If there are no migrations and we're in Development, fall back to EnsureCreated() so Docker/local dev gets tables created.
        var compiledMigrations = db.Database.GetMigrations();
        if (compiledMigrations != null && compiledMigrations.Any())
        {
            db.Database.Migrate();
            logger.LogInformation("Database migrations applied at startup.");
        }
        else if (app.Environment.IsDevelopment())
        {
            var created = db.Database.EnsureCreated();
            logger.LogInformation(created
                ? "No EF migrations found — database created via EnsureCreated() (development only)."
                : "No EF migrations found — database already exists (EnsureCreated() no-op).");
        }
        else
        {
            logger.LogWarning("No EF migrations found in assembly. Add migrations before running in Production.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
