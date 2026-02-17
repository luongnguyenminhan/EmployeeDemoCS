PROJECT = EmployeeDemoCS/EmployeeDemoCS.csproj
HTTP_URL = http://localhost:5087
HTTPS_URL = https://localhost:7040

.PHONY: help build run run-https swagger clean

help:
	@echo "Usage: make <target>"
	@echo ""
	@echo "Targets:"
	@echo "  build        dotnet build"
	@echo "  run          run using HTTP (http://localhost:5087)"
	@echo "  run-https    run using HTTPS launch profile (https://localhost:7040)"
	@echo "  swagger      start app (https) and show Swagger UI URL"
	@echo "  clean        dotnet clean"

build:
	dotnet build $(PROJECT)

run:
	dotnet run --project $(PROJECT) --urls "$(HTTP_URL)"

run-https:
	dotnet run --project $(PROJECT) --launch-profile "https"

swagger: run-https
	@echo "Swagger UI: $(HTTPS_URL)/swagger/index.html"

clean:
	dotnet clean $(PROJECT)
