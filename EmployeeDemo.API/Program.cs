var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Or builder.Services.AddEndpointsApiExplorer(); for minimal APIs
builder.Services.AddSwaggerGen(); // Adds the Swagger generator

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Serves the API documentation as OpenAPI JSON
    app.UseSwaggerUI(); // Enables the embedded Swagger UI tool
}

app.UseAuthorization();

app.MapControllers(); // Or map your minimal API endpoints

app.Run();
