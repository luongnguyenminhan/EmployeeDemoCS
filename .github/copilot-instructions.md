# Copilot instructions for EmployeeDemoCS

Purpose
- Short: help AI agents become productive quickly in this repo (minimal ASP.NET Core demo).

Big picture / architecture
- Single-project ASP.NET Core 10 minimal API (TargetFramework: `net10.0`).
- No DB, no external services — currently a self-contained demo web API exposing `GET /weatherforecast`.
- OpenAPI is enabled in Development via `AddOpenApi()` / `MapOpenApi()`.

Key files to read first
- `Program.cs` — entry point; Minimal-API style, example endpoint: `GET /weatherforecast` and `WeatherForecast` `record`.
- `EmployeeDemoCS.csproj` — target framework, package references (uses `Microsoft.AspNetCore.OpenApi` v10.x).
- `Properties/launchSettings.json` — launch profiles and default ports (`http` -> http://localhost:5087; `https` -> https://localhost:7040).
- `appsettings.json` / `appsettings.Development.json` — runtime configuration & logging defaults.
- `EmployeeDemoCS.http` — ready-made request to verify the running app.

How to build / run / verify (exact commands)
- Build: `dotnet build ./EmployeeDemoCS/EmployeeDemoCS.csproj`
- Run (HTTPS profile): `dotnet run --project ./EmployeeDemoCS/EmployeeDemoCS.csproj --launch-profile "https"`
- Run (explicit URL): `dotnet run --project ./EmployeeDemoCS/EmployeeDemoCS.csproj --urls "http://localhost:5087"`
- Quick verify: `curl http://localhost:5087/weatherforecast` or use the `EmployeeDemoCS.http` request file in the repo.

Project-specific conventions & patterns
- Minimal-API pattern in `Program.cs` — prefer adding small endpoints there for demo features.
- DTOs use `record` types (see `WeatherForecast`); `Nullable` is enabled — use nullable annotations consistently.
- `ImplicitUsings` is enabled; avoid adding redundant `using` statements.
- OpenAPI registration is intentionally limited to Development: keep `MapOpenApi()` behind `app.Environment.IsDevelopment()`.

When you add code
- Small endpoint or DTO: add to `Program.cs` or a new `.cs` file under the project root; keep namespace consistent with project name.
- New dependencies: add `PackageReference` to `EmployeeDemoCS.csproj` and run `dotnet restore` / `dotnet build`.
- Config keys: add to `appsettings.Development.json` for local testing and do NOT commit secrets.
- If adding tests, create a new test project targeting `net10.0` next to the solution.

Debugging tips
- Use VS/VSCode launch profile (profiles defined in `Properties/launchSettings.json`).
- Logs: configured via `appsettings*.json`; runtime logs appear in console when running with `dotnet run`.
- OpenAPI UI available only in Development due to `MapOpenApi()` guard.

Integration / external-dependency notes
- Currently none. Any added external integration MUST be documented in `appsettings.json` and the README/PR.

PR checklist for changes
- Builds cleanly: `dotnet build` ✅
- Behavior verified via `EmployeeDemoCS.http` or curl ✅
- Keep the Minimal-API style consistent; follow `record` DTO pattern ✅

Examples from this repo (copy/paste-ready)
- OpenAPI registration (in `Program.cs`):
  - `builder.Services.AddOpenApi();`  and  `if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }`
- Weather endpoint pattern:
  - `app.MapGet("/weatherforecast", () => /* returns WeatherForecast[] */).WithName("GetWeatherForecast");`

What not to do
- Don't change `TargetFramework` or remove `ImplicitUsings`/`Nullable` without a clear reason.
- Don't commit secrets to `appsettings.Development.json`.

Questions for you
- Any additional runtime profiles, CI pipelines, or integration endpoints I should document?

---
If anything here is unclear or you want more detail (ports, new endpoints, or test scaffolding), tell me which part to expand. 