# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy solution and project files for efficient restore
COPY EmployeeDemo.slnx ./
COPY EmployeeDemo.API/EmployeeDemo.API.csproj EmployeeDemo.API/
COPY EmployeeDemo.Application/EmployeeDemo.Application.csproj EmployeeDemo.Application/
COPY EmployeeDemo.Domain/EmployeeDemo.Domain.csproj EmployeeDemo.Domain/
COPY EmployeeDemo.Infrastructure/EmployeeDemo.Infrastructure.csproj EmployeeDemo.Infrastructure/

RUN dotnet restore EmployeeDemo.slnx

# copy everything and publish
COPY . .
# Ensure publish performs restore inside the container (fixes missing package errors during publish)
RUN dotnet publish EmployeeDemo.API/EmployeeDemo.API.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "EmployeeDemo.API.dll"]
