# copilot-instructions — EmployeeDemoCS

Mục đích: hướng dẫn ngắn, thực tế để một AI coding agent (hoặc công tác viên) trở nên ngay lập tức hữu dụng trong repository này.

## Tóm tắt kiến trúc (big picture)
- Kiến trúc: **Clean Architecture** (4 layer): `EmployeeDemo.API` (presentation) → `EmployeeDemo.Application` (use-cases, interfaces) → `EmployeeDemo.Domain` (business rules) ; `EmployeeDemo.Infrastructure` cung cấp cài đặt (persistence, external clients, DI).
- Quy tắc phụ thuộc: API → Application → Domain. Infrastructure *implements* interfaces từ Application; Domain không reference framework.
- Entry points / ví dụ: `EmployeeDemo.API/Program.cs`, `EmployeeDemo.API/Controllers/WeatherForecastController.cs`.

## Những chỗ cần đọc đầu tiên
- `Program.cs` — cấu hình DI, middleware, swagger và entrypoint.
- `Controllers/WeatherForecastController.cs` — pattern controller/minimal API sample.
- `EmployeeDemo.Application/UseCases/` — nơi đặt business use-cases / handlers.
- `EmployeeDemo.Domain/Entities/` — nơi đặt các Entity/ValueObject.
- `EmployeeDemo.Infrastructure/Persistence/Contexts/` & `DependencyInjection/` — DbContext/DI registration.
- `Makefile` và `README.md` — lệnh build/run/healthcheck.

## Quy ước & patterns cụ thể của dự án
- Use-case layout: `Application/UseCases/Commands` và `.../Queries`.
- DTOs: `API/DTOs/Requests` và `API/DTOs/Responses`; Application cũng có `DTOs` cho contracts nội bộ.
- Interfaces: khai báo trong `Application/Interfaces`, implement trong `Infrastructure/Repositories`.
- Mapping config: `Application/Mappings` (AutoMapper / Mapster style).
- DI: tạo extension trong `Infrastructure/DependencyInjection` và gọi nó trong `Program.cs`.
- Naming / namespaces: giữ `EmployeeDemo.{API|Application|Domain|Infrastructure}`.

## Công việc thường dùng (developer workflows)
- Build: `dotnet build ./EmployeeDemoCS/EmployeeDemoCS.csproj`
- Run (HTTPS): `make run-https` hoặc `dotnet run --project ./EmployeeDemoCS/EmployeeDemoCS.csproj --launch-profile "https"`
- Run (HTTP): `make run`
- Swagger UI: `https://localhost:7040/swagger/index.html`
- EF migrations (nếu thêm persistence):
  `dotnet ef migrations add <Name> -c AppDbContext -p ../EmployeeDemo.Infrastructure/EmployeeDemo.Infrastructure.csproj -s EmployeeDemo.API.csproj -o Persistence/Migrations`

## Hướng dẫn cho AI agents — phải tuân thủ
- Không chỉnh sửa `bin/` hoặc `obj/` hoặc file generated (*.g.cs, *.runtimeconfig.json).
- Khi thêm repository: tạo interface trong `Application/Interfaces`, implement trong `Infrastructure/Repositories`, thêm registration trong `Infrastructure/DependencyInjection` và unit test tương ứng.
- Thay đổi API public (endpoint/contract) yêu cầu: cập nhật `API/DTOs`, sửa `Application/DTOs`/mappings, và thêm test/hoặc `EmployeeDemo.API.http` ví dụ.
- Mọi thay đổi DB/Persistence phải kèm migration (nếu dùng EF Core) và hướng dẫn update DB trong README.
- Giữ các thay đổi nhỏ, atomic; mỗi PR nên kèm mô tả ngắn, files thay đổi, và lệnh để chạy locally.

## Các task “safe” mà agent có thể làm tự động
- Tạo folders / scaffolding theo structure Clean Architecture (đã có sẵn).
- Tạo DTO + UseCase handler stub + controller stub — nhưng trước khi push **yêu cầu phê duyệt** người dùng.
- Thêm DI extension skeleton trong `Infrastructure/DependencyInjection`.

## Tài liệu tham khảo trong repo
- `README.md` — quick start, commands.
- `Makefile` — build/run targets.
- `EmployeeDemo.API/Program.cs`, `Controllers/WeatherForecastController.cs` — ví dụ thực thi.

## Khi cần trợ giúp từ con người
- Nếu task thay đổi contract API, DB schema hoặc cross-layer boundaries — **dừng lại** và hỏi reviewer.

---

Nếu bạn muốn, tôi sẽ: (A) tạo file mẫu cho một repository + DbContext + DI extension, hoặc (B) chỉ tạo một controller + DTO mẫu. Chọn A hoặc B hoặc trả lời "không".