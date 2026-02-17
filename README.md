# EmployeeDemoCS

Minimal ASP.NET Core 10 example project (Minimal API).

Quick start
- Build: `dotnet build ./EmployeeDemoCS/EmployeeDemoCS.csproj`
- Run (HTTPS): `make run-https` or `dotnet run --project ./EmployeeDemoCS/EmployeeDemoCS.csproj --launch-profile "https"`
- Run (HTTP): `make run` or `dotnet run --project ./EmployeeDemoCS/EmployeeDemoCS.csproj --urls "http://localhost:5087"`
- Swagger UI: `https://localhost:7040/swagger/index.html`
- Verify: `curl http://localhost:5087/weatherforecast` or use `EmployeeDemoCS/EmployeeDemoCS.http`

AI agent guidance
- See `.github/copilot-instructions.md` for repository-specific instructions for AI coding agents and contributors.
