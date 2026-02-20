# .NET Project Structure and Build Configuration

## When to Use This Skill

Use this skill when:
- **Setting up a new Clean Architecture project** — Organizing code into layered, testable components
- Implementing 4-layer architecture (API, Application, Domain, Infrastructure)
- **Defining layer responsibilities and folder structures** — Understanding where to place files
- **Establishing naming conventions** — Services, repositories, DTOs, ViewModels, etc.
- **Setting up dependency injection patterns** — DependencyInjection.cs extensions
- Creating scalable .NET solutions with clear separation of concerns
- Configuring centralized build properties across multiple projects
- Implementing central package version management
- Setting up SourceLink for debugging and NuGet packages
- Automating version management with release notes
- Pinning SDK versions for consistent builds

## Related Skills

- **`dotnet-local-tools`** - Managing local .NET tools with dotnet-tools.json
- **`microsoft-extensions-configuration`** - Configuration validation patterns

---

## Naming Conventions Guide

This skill uses generic placeholders in code examples to be reusable across projects. Replace these with your actual names:

| Placeholder | Meaning | Examples |
|-------------|---------|----------|
| `<SOLUTION_NAME>` | Your project's root namespace | `EmployeeDemo`, `OrderSystem`, `BlogApp` |
| `<DOMAIN_ENTITY>` | A business entity class name | `Product`, `Account`, `Invoice`, `Customer` |
| `<ENTITY>` | Short form of entity name (used in class names) | `Product`, `Account`, `Invoice` |
| `<entities>` | Lowercase plural of entity (used in URLs) | `products`, `accounts`, `invoices` |
| `<entity>` | Lowercase singular of entity (used in variables) | `product`, `account`, `invoice` |
| `<ANOTHER_ENTITY>` | Second distinct entity in examples | `Account`, `Order`, `Student` |
| `<THIRD_ENTITY>` | Third distinct entity in examples | `Student`, `Invoice`, `Payment` |
| `<PROPERTY_NAME>` | A property representing an entity's main name | `ProductName`, `AccountEmail`, `CustomerName` |
| `<PROPERTY_DESCRIPTION>` | A property for descriptive text | `ProductDescription`, `AccountBio` |
| `<PROPERTY_VALUE>` | A numeric or core property of an entity | `ProductPrice`, `AccountBalance`, `OrderTotal` |
| `<STATUS_ENUM>` | An enumeration for status/roles | `RoleEnum`, `StatusEnum`, `PriorityEnum` |
| `<STATUS_VALUE_1>` | Enum value | `Admin`, `Pending`, `High` |
| `<CONTEXT_SERVICE>` | A service providing HTTP context data | `ClaimsService`, `UserContextService` |
| `<TIME_SERVICE>` | A service providing current time | `CurrentTime`, `SystemClock` |
| `<UTILITY_DOMAIN>` | Domain for utility functions | `Auth`, `Token`, `Validation` |
| `<EXTERNAL_SERVICE_NAME>` | Third-party integration service | `EmailService`, `SMSService`, `PaymentService` |
| `<EXTERNAL_API>` | External API client | `PaymentGateway`, `WeatherAPI`, `Stripe` |
| `<CROSS_CUTTING_CONCERN>` | Middleware concern | `Performance`, `Logging`, `Security` |
| `<PROJECT_NAME>` | Full project name (for file names) | `EmployeeDemo.API`, `OrderSystem.API` |
| `<TOKEN_MODEL>` | Token representation class | `TokenModel`, `JwtToken`, `AuthToken` |
| `<SETTINGS_NAME>` | Configuration class name | `JwtSettings`, `DatabaseSettings` |
| `<OPERATION>` | Business operation name | `Login`, `Create`, `Register` |

---

## Solution File Format (.slnx)

The `.slnx` format is the modern XML-based solution file format introduced in .NET 9. It replaces the traditional `.sln` format.

### Benefits Over Traditional .sln

| Aspect | .sln (Legacy) | .slnx (Modern) |
|--------|---------------|----------------|
| Format | Custom text format | Standard XML |
| Readability | GUIDs, cryptic syntax | Clean, human-readable |
| Version control | Hard to diff/merge | Easy to diff/merge |
| Editing | IDE required | Any text editor |

### Version Requirements

| Tool | Minimum Version |
|------|-----------------|
| .NET SDK | 9.0.200 |
| Visual Studio | 17.13 |
| MSBuild | Visual Studio Build Tools 17.13 |

**Note:** Starting with .NET 10, `dotnet new sln` creates `.slnx` files by default. In .NET 9, you must explicitly migrate or specify the format.

### Example .slnx File

```xml
<Solution>
  <Folder Name="/build/">
    <File Path="Directory.Build.props" />
    <File Path="Directory.Packages.props" />
    <File Path="global.json" />
    <File Path="NuGet.Config" />
    <File Path="README.md" />
  </Folder>
  <Folder Name="/src/">
    <Project Path="src/MyApp/MyApp.csproj" />
    <Project Path="src/MyApp.Core/MyApp.Core.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/MyApp.Tests/MyApp.Tests.csproj" />
  </Folder>
</Solution>
```

### Migrating from .sln to .slnx

Use the `dotnet sln migrate` command to convert existing solutions:

```bash
# Migrate a specific solution file
dotnet sln MySolution.sln migrate

# Or if only one .sln exists in the directory, just run:
dotnet sln migrate
```

**Important:** Do not keep both `.sln` and `.slnx` files in the same repository. This causes issues with automatic solution detection and can lead to sync problems. After migration, delete the old `.sln` file.

You can also migrate in Visual Studio:
1. Open the solution
2. Select the Solution in Solution Explorer
3. Go to **File > Save Solution As...**
4. Change "Save as type" to **Xml Solution File (*.slnx)**

### Creating a New .slnx Solution

```bash
# .NET 10+: Creates .slnx by default
dotnet new sln --name MySolution

# .NET 9: Specify the format explicitly
dotnet new sln --name MySolution --format slnx

# Add projects (works the same for both formats)
dotnet sln add src/MyApp/MyApp.csproj
```

### Recommendation

**If you're using .NET 9.0.200 or later, migrate your solutions to .slnx.** The benefits are significant:
- Dramatically fewer merge conflicts (no random GUIDs changing)
- Human-readable and editable in any text editor
- Consistent with modern `.csproj` format
- Better diff/review experience in pull requests

---

## Directory.Build.props

`Directory.Build.props` provides centralized build configuration that applies to all projects in a directory tree. Place it at the solution root.

### Complete Example

```xml
<Project>
  <!-- Metadata -->
  <PropertyGroup>
    <Authors>Your Team</Authors>
    <Company>Your Company</Company>
    <!-- Dynamic copyright year - updates automatically -->
    <Copyright>Copyright © 2020-$([System.DateTime]::Now.Year) Your Company</Copyright>
    <Product>Your Product</Product>
    <PackageProjectUrl>https://github.com/yourorg/yourrepo</PackageProjectUrl>
    <RepositoryUrl>https://github.com/yourorg/yourrepo</RepositoryUrl>
    <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
    <PackageTags>your;tags;here</PackageTags>
  </PropertyGroup>

  <!-- C# Language Settings -->
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <NoWarn>$(NoWarn);CS1591</NoWarn> <!-- Missing XML comments -->
  </PropertyGroup>

  <!-- Version Management -->
  <PropertyGroup>
    <VersionPrefix>1.0.0</VersionPrefix>
    <PackageReleaseNotes>See RELEASE_NOTES.md</PackageReleaseNotes>
  </PropertyGroup>

  <!-- Target Framework Definitions (reusable properties) -->
  <PropertyGroup>
    <NetStandardLibVersion>netstandard2.0</NetStandardLibVersion>
    <NetLibVersion>net8.0</NetLibVersion>
    <NetTestVersion>net9.0</NetTestVersion>
  </PropertyGroup>

  <!-- SourceLink Configuration -->
  <PropertyGroup>
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
    <EmbedUntrackedSources>true</EmbedUntrackedSources>
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
  </ItemGroup>

  <!-- NuGet Package Assets -->
  <ItemGroup>
    <None Include="$(MSBuildThisFileDirectory)logo.png" Pack="true" PackagePath="\" />
    <None Include="$(MSBuildThisFileDirectory)README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <PropertyGroup>
    <PackageIcon>logo.png</PackageIcon>
    <PackageReadmeFile>README.md</PackageReadmeFile>
  </PropertyGroup>

  <!-- Global Using Statements -->
  <ItemGroup>
    <Using Include="System.Collections.Immutable" />
  </ItemGroup>
</Project>
```

### Key Patterns

#### Dynamic Copyright Year

```xml
<Copyright>Copyright © 2020-$([System.DateTime]::Now.Year) Your Company</Copyright>
```

Uses MSBuild property functions to insert current year at build time. No manual updates needed.

#### Reusable Target Framework Properties

Define target frameworks once, reference everywhere:

```xml
<!-- In Directory.Build.props -->
<PropertyGroup>
  <NetLibVersion>net8.0</NetLibVersion>
  <NetTestVersion>net9.0</NetTestVersion>
</PropertyGroup>

<!-- In MyApp.csproj -->
<PropertyGroup>
  <TargetFramework>$(NetLibVersion)</TargetFramework>
</PropertyGroup>

<!-- In MyApp.Tests.csproj -->
<PropertyGroup>
  <TargetFramework>$(NetTestVersion)</TargetFramework>
</PropertyGroup>
```

#### SourceLink for NuGet Packages

SourceLink enables step-through debugging of NuGet packages:

```xml
<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<ItemGroup>
  <!-- Choose the right provider for your source control -->
  <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
  <!-- Or: Microsoft.SourceLink.AzureRepos.Git -->
  <!-- Or: Microsoft.SourceLink.GitLab -->
  <!-- Or: Microsoft.SourceLink.Bitbucket.Git -->
</ItemGroup>
```

---

## Directory.Packages.props - Central Package Management

Central Package Management (CPM) provides a single source of truth for all NuGet package versions.

### Setup

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <!-- Define version variables for related packages -->
  <PropertyGroup>
    <AkkaVersion>1.5.35</AkkaVersion>
    <AspireVersion>9.1.0</AspireVersion>
  </PropertyGroup>

  <!-- Application Dependencies -->
  <ItemGroup Label="App Dependencies">
    <PackageVersion Include="Akka" Version="$(AkkaVersion)" />
    <PackageVersion Include="Akka.Cluster" Version="$(AkkaVersion)" />
    <PackageVersion Include="Akka.Persistence" Version="$(AkkaVersion)" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
  </ItemGroup>

  <!-- Build/Tooling Dependencies -->
  <ItemGroup Label="Build Dependencies">
    <PackageVersion Include="Microsoft.SourceLink.GitHub" Version="8.0.0" />
  </ItemGroup>

  <!-- Test Dependencies -->
  <ItemGroup Label="Test Dependencies">
    <PackageVersion Include="xunit" Version="2.9.3" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="3.0.1" />
    <PackageVersion Include="FluentAssertions" Version="7.0.0" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageVersion Include="coverlet.collector" Version="6.0.3" />
  </ItemGroup>
</Project>
```

### Consuming Packages (No Version Needed)

```xml
<!-- In MyApp.csproj -->
<ItemGroup>
  <PackageReference Include="Akka" />
  <PackageReference Include="Akka.Cluster" />
  <PackageReference Include="Microsoft.Extensions.Hosting" />
</ItemGroup>

<!-- In MyApp.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="xunit" />
  <PackageReference Include="FluentAssertions" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" />
</ItemGroup>
```

### Benefits

1. **Single source of truth** - All versions in one file
2. **No version drift** - All projects use same versions
3. **Easy updates** - Change once, applies everywhere
4. **Grouped packages** - Version variables for related packages (e.g., all Akka packages)

---

## global.json - SDK Version Pinning

Pin the .NET SDK version for consistent builds across all environments.

```json
{
  "sdk": {
    "version": "9.0.200",
    "rollForward": "latestFeature"
  }
}
```

### Roll Forward Policies

| Policy | Behavior |
|--------|----------|
| `disable` | Exact version required |
| `patch` | Same major.minor, latest patch |
| `feature` | Same major, latest minor.patch |
| `latestFeature` | Same major, latest feature band |
| `minor` | Same major, latest minor |
| `latestMinor` | Same major, latest minor |
| `major` | Latest SDK (not recommended) |

**Recommended:** `latestFeature` - Allows patch updates within the same feature band.

---

## Version Management with RELEASE_NOTES.md

### Release Notes Format

```markdown
#### 1.2.0 January 15th 2025 ####

- Added new feature X
- Fixed bug in Y
- Improved performance of Z

#### 1.1.0 December 10th 2024 ####

- Initial release with features A, B, C
```

### Parsing Script (getReleaseNotes.ps1)

```powershell
function Get-ReleaseNotes {
    param (
        [Parameter(Mandatory=$true)]
        [string]$MarkdownFile
    )

    $content = Get-Content -Path $MarkdownFile -Raw
    $sections = $content -split "####"

    $result = [PSCustomObject]@{
        Version      = $null
        Date         = $null
        ReleaseNotes = $null
    }

    if ($sections.Count -ge 3) {
        $header = $sections[1].Trim()
        $releaseNotes = $sections[2].Trim()

        $headerParts = $header -split " ", 2
        if ($headerParts.Count -eq 2) {
            $result.Version = $headerParts[0]
            $result.Date = $headerParts[1]
        }

        $result.ReleaseNotes = $releaseNotes
    }

    return $result
}
```

### Version Bump Script (bumpVersion.ps1)

```powershell
function UpdateVersionAndReleaseNotes {
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$ReleaseNotesResult,
        [Parameter(Mandatory=$true)]
        [string]$XmlFilePath
    )

    $xmlContent = New-Object XML
    $xmlContent.Load($XmlFilePath)

    # Update VersionPrefix
    $versionElement = $xmlContent.SelectSingleNode("//VersionPrefix")
    $versionElement.InnerText = $ReleaseNotesResult.Version

    # Update PackageReleaseNotes
    $notesElement = $xmlContent.SelectSingleNode("//PackageReleaseNotes")
    $notesElement.InnerText = $ReleaseNotesResult.ReleaseNotes

    $xmlContent.Save($XmlFilePath)
}
```

### Build Script (build.ps1)

```powershell
# Load helper scripts
. "$PSScriptRoot\scripts\getReleaseNotes.ps1"
. "$PSScriptRoot\scripts\bumpVersion.ps1"

# Parse release notes and update Directory.Build.props
$releaseNotes = Get-ReleaseNotes -MarkdownFile (Join-Path -Path $PSScriptRoot -ChildPath "RELEASE_NOTES.md")
UpdateVersionAndReleaseNotes -ReleaseNotesResult $releaseNotes -XmlFilePath (Join-Path -Path $PSScriptRoot -ChildPath "Directory.Build.props")

Write-Output "Updated to version $($releaseNotes.Version)"
```

### CI/CD Integration

```yaml
# GitHub Actions example
- name: Update version from release notes
  shell: pwsh
  run: ./build.ps1

- name: Build
  run: dotnet build -c Release

- name: Pack with tag version
  run: dotnet pack -c Release /p:PackageVersion=${{ github.ref_name }}

- name: Push to NuGet
  run: dotnet nuget push **/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

---

## NuGet.Config

Configure NuGet sources and behavior:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <solution>
    <add key="disableSourceControlIntegration" value="true" />
  </solution>

  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <!-- Add private feeds if needed -->
    <!-- <add key="MyCompany" value="https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json" /> -->
  </packageSources>
</configuration>
```

**Key Settings:**
- `<clear />` - Remove inherited/default sources for reproducible builds
- `disableSourceControlIntegration` - Prevents TFS/Git integration issues

---

## Complete Project Structure

```
MySolution/
├── .config/
│   └── dotnet-tools.json           # Local .NET tools
├── .github/
│   └── workflows/
│       ├── pr-validation.yml       # PR checks
│       └── release.yml             # NuGet publishing
├── scripts/
│   ├── getReleaseNotes.ps1         # Parse RELEASE_NOTES.md
│   └── bumpVersion.ps1             # Update Directory.Build.props
├── src/
│   ├── MyApp/
│   │   └── MyApp.csproj
│   └── MyApp.Core/
│       └── MyApp.Core.csproj
├── tests/
│   └── MyApp.Tests/
│       └── MyApp.Tests.csproj
├── Directory.Build.props           # Centralized build config
├── Directory.Packages.props        # Central package versions
├── MySolution.slnx                 # Modern solution file
├── global.json                     # SDK version pinning
├── NuGet.Config                    # Package source config
├── build.ps1                       # Build orchestration
├── RELEASE_NOTES.md                # Version history
├── README.md                       # Project documentation
└── logo.png                        # Package icon
```

---

## Clean Architecture Layers

Clean Architecture divides a project into **4 concentric layers**, each with distinct responsibilities and dependency rules. Dependencies always point **inward** only—outer layers can reference inner layers, but never the reverse.

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation (API)                      │
│          Controllers, DTOs, Middlewares, Filters            │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                  Application (Use-Cases)                    │
│    Services, Interfaces, DTOs, Validators, Mappings         │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                  Domain (Business Rules)                    │
│           Entities, ValueObjects, Enums, Exceptions         │
└─────────────────────────────────────────────────────────────┘
```

**Infrastructure** (bottom layer, not shown) implements interfaces defined by Application and Domain. It contains persistence, external services, and dependency injection registration.

---

### Layer 1: <PROJECT_NAME>.Domain — Enterprise Business Rules

**Purpose:** Core business logic that is independent of any framework, database, or external tool. Domain logic should be the most stable part of the codebase.

**Key Principle:** No dependencies on external frameworks or libraries (except System).

#### Folder Structure

```
Domain/
├── Entities/
│   ├── <DOMAIN_ENTITY>.cs           # Aggregate root with properties and validation
│   ├── <ANOTHER_ENTITY>.cs          # Business entity
│   ├── <THIRD_ENTITY>.cs            # Business entity
│   └── BaseEntity.cs                # Abstract base with common audit fields
├── Enums/
│   └── <STATUS_ENUM>.cs             # Domain-specific enumerations (Admin, User, etc.)
├── ValueObjects/            # (Optional) Domain concepts with value semantics
├── Exceptions/              # (Optional) Business rule violations
└── Services/                # (Optional) Pure business logic that doesn't fit in entities
```

#### File Patterns

**BaseEntity.cs** — Abstract base class with audit trail:
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? ModificationDate { get; set; }
    public Guid? ModificationBy { get; set; }
    public DateTime? DeletionDate { get; set; }
    public Guid? DeleteBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

All entities inherit from `BaseEntity` to maintain consistent audit trails.

**Entity.cs** — Business entity with properties and invariant logic:
```csharp
public class <DOMAIN_ENTITY> : BaseEntity
{
    public string <PROPERTY_NAME> { get; set; } = "";
    public string <PROPERTY_DESCRIPTION> { get; set; } = "";
    public int <PROPERTY_VALUE> { get; set; }
    
    // Domain logic (validations, state transitions) lives here
    public void Update<PROPERTY_NAME>(int new<PROPERTY_VALUE>)
    {
        if (new<PROPERTY_VALUE> <= 0) throw new InvalidOperationException("<PROPERTY_NAME> must be positive");
        <PROPERTY_VALUE> = new<PROPERTY_VALUE>;
    }
}
```

**Enums.cs** — Domain-specific enumerations:
```csharp
public enum <STATUS_ENUM>
{
    <STATUS_VALUE_1> = 1,
    <STATUS_VALUE_2> = 2,
    <STATUS_VALUE_3> = 3
}
```

#### When to Add Files

Add a file to Domain when:
- It represents a core business concept (Entity, ValueObject)
- It contains pure business logic that should not depend on databases or HTTP
- It defines a fundamental enumeration or exception in the business domain

#### What NOT to Put Here

- ❌ Repositories or database queries
- ❌ HTTP client calls or external service integrations
- ❌ ASP.NET/Entity Framework attributes or dependencies
- ❌ Application-specific services or DTOs

---

### Layer 2: <PROJECT_NAME>.Application — Use-Cases and Application Services

**Purpose:** Define application-specific business logic and act as the integration layer. Application orchestrates Domain entities and coordinates with Infrastructure through interfaces.

**Key Principle:** Application defines what the system *does* and *what interfaces* must be implemented. It doesn't implement them—Infrastructure does.

#### Folder Structure

```
Application/
├── Interfaces/                          # Port definitions (repositories, external services)
│   ├── I<ENTITY>Service.cs
│   ├── I<ANOTHER_ENTITY>Service.cs
│   ├── I<THIRD_ENTITY>Service.cs
│   ├── I<CONTEXT_SERVICE>.cs
│   ├── I<TIME_SERVICE>.cs
│   └── (more service interfaces as needed)
├── Repositories/                        # Repository port interfaces
│   ├── IGenericRepository.cs            # Base repository contract
│   ├── I<ENTITY>Repository.cs
│   ├── I<ANOTHER_ENTITY>Repository.cs
│   ├── I<THIRD_ENTITY>Repository.cs
│   └── (more repository interfaces)
├── Services/                            # Application service implementations
│   ├── <ENTITY>Service.cs                # Orchestrates <ENTITY>-related use-cases
│   ├── <ANOTHER_ENTITY>Service.cs
│   ├── <THIRD_ENTITY>Service.cs
│   ├── <TIME_SERVICE>.cs                   # Adapter for time (delegates to Infrastructure impl)
│   └── (more application services)
├── ViewModels/                          # DTO-like objects for data transfer
│   ├── <ENTITY>ViewModels/
│   │   └── <ENTITY>DTO.cs
│   ├── <ANOTHER_ENTITY>ViewModels/
│   │   ├── Create<ANOTHER_ENTITY>ViewModel.cs
│   │   └── Update<ANOTHER_ENTITY>ViewModel.cs
│   ├── ResponseModels/
│   │   ├── ResponseModel.cs             # Generic response wrapper
│   │   └── <OPERATION>ResponseModel.cs        # Operation-specific response
│   └── <TOKEN_MODEL>.cs                    # Token representation
├── Commons/                             # Shared application utilities
│   ├── Pagination.cs                    # Generic pagination wrapper
│   ├── PaginationParameter.cs           # Query parameter model
│   └── AppConfiguration.cs              # App setting constants
├── Utils/                               # Helper functions
│   ├── <UTILITY_DOMAIN>Tools.cs                   # Utility functions
│   ├── Generate<TOKEN_TYPE>.cs              # Token generation
│   └── <TOKEN_TYPE>Tools.cs                    # Token parsing/validation
├── IUnitOfWork.cs                       # Unit of Work pattern interface
```

#### File Patterns

**I<ENTITY>Service.cs** — Service interface defining use-cases:
```csharp
public interface I<ENTITY>Service
{
    Task<<DOMAIN_ENTITY>> Get<ENTITY>Async(Guid <entity>Id);
    Task<Pagination<<DOMAIN_ENTITY>>> Get<ENTITY>PaginationAsync(PaginationParameter paginationParameter);
    Task<<DOMAIN_ENTITY>> Create<ENTITY>Async(Create<ENTITY>ViewModel <entity>);
}
```

**<ENTITY>Service.cs** — Service implementation (orchestrates repositories):
```csharp
public class <ENTITY>Service : I<ENTITY>Service
{
    private readonly I<ENTITY>Repository _<entity>Repository;
    
    public <ENTITY>Service(I<ENTITY>Repository <entity>Repository)
    {
        _<entity>Repository = <entity>Repository;
    }
    
    public async Task<<DOMAIN_ENTITY>> Create<ENTITY>Async(Create<ENTITY>ViewModel <entity>)
    {
        // Application logic: coordinate multiple operations
        var new<ENTITY> = new <DOMAIN_ENTITY> 
        { 
            <PROPERTY_NAME> = <entity>.<PROPERTY_NAME>,
            <PROPERTY_DESCRIPTION> = <entity>.<PROPERTY_DESCRIPTION>,
            <PROPERTY_VALUE> = <entity>.<PROPERTY_VALUE>
        };
        
        await _<entity>Repository.AddAsync(new<ENTITY>);
        return new<ENTITY>;
    }
}
```

**I<ENTITY>Repository.cs** — Repository port (contract for Infrastructure):
```csharp
public interface I<ENTITY>Repository : IGenericRepository<<DOMAIN_ENTITY>>
{
    // Add <ENTITY>-specific query methods here
    Task<<DOMAIN_ENTITY>?> GetBy<UNIQUE_PROPERTY>Async(string <uniqueProperty>);
}
```

**Create<ENTITY>ViewModel.cs** — Input DTO:
```csharp
public class Create<ENTITY>ViewModel
{
    public string <PROPERTY_NAME> { get; set; } = "";
    public string <PROPERTY_DESCRIPTION> { get; set; } = "";
    public int <PROPERTY_VALUE> { get; set; }
}
```

**ResponseModel.cs** — Response wrapper:
```csharp
public class ResponseModel
{
    public bool Status { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}
```

**IUnitOfWork.cs** — Coordinates multiple repositories:
```csharp
public interface IUnitOfWork
{
    I<ENTITY>Repository <ENTITY>Repository { get; }
    I<ANOTHER_ENTITY>Repository <ANOTHER_ENTITY>Repository { get; }
    I<THIRD_ENTITY>Repository <THIRD_ENTITY>Repository { get; }
    
    Task<int> SaveChangeAsync();
}
```

**Pagination.cs** — Generic pagination container:
```csharp
public class Pagination<T> : List<T>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
```

#### When to Add Files

Add to Application when:
- **Interfaces/** — Defining a port for Infrastructure to implement (Repository, External Service)
- **Services/** — Orchestrating use-cases, coordinating multiple repositories/services
- **ViewModels/** — Defining input/output DTOs for API contracts
- **Commons/** — Shared utilities (Pagination, Constants, Configuration)

#### What NOT to Put Here

- ❌ Database implementation (DbContext, migrations)
- ❌ HTTP client implementations  
- ❌ External service integrations
- ❌ Infrastructure-specific attributes (DataAnnotations, Entity Framework configurations)

#### Dependency Rules

- ✅ Application **can reference** Domain
- ✅ Application **can reference** other Application services
- ❌ Application **cannot reference** Infrastructure
- ❌ Application **cannot reference** API

---

### Layer 3: <PROJECT_NAME>.API — Presentation / Web Layer

**Purpose:** Entry point for HTTP requests. Controllers receive requests, call Application services, and return responses. This layer is responsible for HTTP concerns only (status codes, content negotiation).

**Key Principle:** Keep controllers thin—they should orchestrate, not contain business logic.

#### Folder Structure

```
API/
├── Controllers/                         # HTTP endpoints
│   ├── <ENTITY>sController.cs
│   ├── <ANOTHER_ENTITY>sController.cs
│   ├── <THIRD_ENTITY>sController.cs
│   └── HealthCheckController.cs
├── Middlewares/                         # Request/Response pipeline
│   ├── GlobalExceptionMiddleware.cs     # Centralized error handling
│   ├── <CROSS_CUTTING_CONCERN>Middleware.cs         # Request timing/logging
│   └── (more middleware)
├── Services/                            # Adapter services for HTTP context
│   └── <CONTEXT_SERVICE>.cs                 # Extract user info from HTTP context
├── Filters/                             # (Optional) Action/Exception filters
├── HealthChecks/                        # (Optional) Health check endpoints
├── Program.cs                           # Startup configuration & DI setup
├── DependencyInjection.cs               # API-layer DI registration
├── appsettings.json                     # Production configuration
├── appsettings.Development.json         # Development configuration
├── <PROJECT_NAME>.API.http                # REST client examples (VS Code/Rider)
└── WeatherForecast.cs                   # Sample model (can be removed)
```

#### File Patterns

**<ENTITY>sController.cs** — Thin controller that orchestrates:
```csharp
[Route("api/<entities>")]
[ApiController]
public class <ENTITY>sController : ControllerBase
{
    private readonly I<ENTITY>Service _<entity>Service;
    
    public <ENTITY>sController(I<ENTITY>Service <entity>Service)
    {
        _<entity>Service = <entity>Service;
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> Create<ENTITY>Async(
        Create<ENTITY>ViewModel <entity>ViewModel, 
        [FromQuery] <STATUS_ENUM> <status>)
    {
        try
        {
            var result = await _<entity>Service.Create<ENTITY>Async(<entity>ViewModel);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseModel { Status = false, Message = ex.Message });
        }
    }
}
```

**GlobalExceptionMiddleware.cs** — Centralized error handling:
```csharp
public class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(
                new ResponseModel 
                { 
                    Status = false, 
                    Message = "Internal server error" 
                });
        }
    }
}
```

**<CONTEXT_SERVICE>.cs** — Adapter extracting claims from HTTP context:
```csharp
public class <CONTEXT_SERVICE> : I<CONTEXT_SERVICE>
{
    private readonly Guid _userId;
    
    public <CONTEXT_SERVICE>(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        var extractedId = <UTILITY_DOMAIN>Tools.GetCurrentUserId(identity);
        _userId = string.IsNullOrEmpty(extractedId) ? Guid.Empty : Guid.Parse(extractedId);
    }
    
    public Guid GetCurrentUserId => _userId;
}
```

**<CROSS_CUTTING_CONCERN>Middleware.cs** — Request/response timing:
```csharp
public class <CROSS_CUTTING_CONCERN>Middleware : IMiddleware
{
    private readonly Stopwatch _stopwatch;
    
    public <CROSS_CUTTING_CONCERN>Middleware(Stopwatch stopwatch)
    {
        _stopwatch = stopwatch;
    }
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _stopwatch.Start();
        await next(context);
        _stopwatch.Stop();
        
        context.Response.Headers.Add("X-<CROSS_CUTTING_CONCERN>-Time", $"{_stopwatch.ElapsedMilliseconds}ms");
    }
}
```

**DependencyInjection.cs** — Register API services:
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddWebAPIService(this IServiceCollection services)
    {
        services.AddSingleton<GlobalExceptionMiddleware>();
        services.AddSingleton<<CROSS_CUTTING_CONCERN>Middleware>();
        services.AddSingleton<Stopwatch>();
        services.AddScoped<I<CONTEXT_SERVICE>, <CONTEXT_SERVICE>>();
        services.AddHttpContextAccessor();
        return services;
    }
}
```

**Program.cs** — Startup and middleware pipeline:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services (from DI extensions)
builder.Services.AddWebAPIService();
builder.Services.AddInfrastructuresService(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use middleware in order
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### When to Add Files

Add to API when:
- **Controllers/** — New HTTP endpoint for a use-case
- **Middlewares/** — Cross-cutting HTTP concerns (logging, auth, error handling)
- **Services/** — Adapting HTTP context to Application interfaces (ClaimsService)
- **<PROJECT_NAME>.API.http** — Examples of API calls (for testing with VS Code/Rider)

#### What NOT to Put Here

- ❌ Repository implementations (belongs in Infrastructure)
- ❌ Domain entities or business logic enforcement
- ❌ Database operations
- ❌ External service integrations (except as adapters)

#### Dependency Rules

- ✅ API **can reference** Application
- ✅ API **can reference** Domain (for types only)
- ❌ API **cannot reference** Infrastructure directly
- ❌ API **cannot reference** other controllers' logic

---

### Layer 4: <PROJECT_NAME>.Infrastructure — Implementation Details

**Purpose:** Implement the ports (interfaces) defined by Application. This layer handles all I/O: databases, external APIs, file systems, email services, etc.

**Key Principle:** All technical implementation details live here. Infrastructure is the most likely to change; Application and Domain are stable.

#### Folder Structure

```
Infrastructure/
├── Persistence/
│   ├── Contexts/
│   │   └── <SOLUTION_NAME>DbContext.cs              # Entity Framework DbContext
│   ├── Configurations/                  # FluentAPI entity mappings (can be in FluentAPIs/)
│   │   ├── <ENTITY>Config.cs
│   │   └── <ANOTHER_ENTITY>Config.cs
│   └── Migrations/                      # EF Core migrations
├── FluentAPIs/                          # (Alt location) Entity type configurations
│   ├── <ENTITY>Config.cs
│   └── <ANOTHER_ENTITY>Config.cs
├── Repositories/                        # Repository implementations
│   ├── GenericRepository.cs             # Base CRUD operations
│   ├── <ENTITY>Repository.cs
│   ├── <ANOTHER_ENTITY>Repository.cs
│   └── <THIRD_ENTITY>Repository.cs
├── Mapper/                              # AutoMapper/Mapster profiles
│   └── MapperConfigProfile.cs
├── Services/                            # (Optional) External service implementations
│   ├── <EXTERNAL_SERVICE_NAME>.cs
│   └── <ANOTHER_EXTERNAL_SERVICE>.cs
├── ExternalClients/                     # (Optional) HTTP clients, SDK wrappers
│   └── <EXTERNAL_API>Client.cs
├── Settings/                            # (Optional) Configuration option classes
│   └── <SETTINGS_NAME>Settings.cs
├── UnitOfWork.cs                        # IUnitOfWork implementation
├── DenpendencyInjection.cs              # DI registration for Infrastructure
└── <SOLUTION_NAME>DbContext.cs                      # (Can be in Persistence/Contexts/)
```

#### File Patterns

**<SOLUTION_NAME>DbContext.cs** — Entity Framework context:
```csharp
public class <SOLUTION_NAME>DbContext : IdentityDbContext<<DOMAIN_ENTITY>>
{
    public <SOLUTION_NAME>DbContext(DbContextOptions<<SOLUTION_NAME>DbContext> options) : base(options) { }
    
    public DbSet<<DOMAIN_ENTITY>> <ENTITY>s { get; set; }
    public DbSet<<ANOTHER_ENTITY>> <ANOTHER_ENTITY>s { get; set; }
    public DbSet<<THIRD_ENTITY>> <THIRD_ENTITY>s { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(<SOLUTION_NAME>DbContext).Assembly);
    }
}
```

**<ENTITY>Config.cs** — FluentAPI entity configuration:
```csharp
public class <ENTITY>Config : IEntityTypeConfiguration<<DOMAIN_ENTITY>>
{
    public void Configure(EntityTypeBuilder<<DOMAIN_ENTITY>> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.<PROPERTY_NAME>).IsRequired().HasMaxLength(100);
        builder.Property(x => x.<PROPERTY_DESCRIPTION>).HasMaxLength(150);
        builder.Property(x => x.<PROPERTY_VALUE>).IsRequired();
    }
}
```

**GenericRepository.cs** — Base repository with common CRUD:
```csharp
public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    protected DbSet<TEntity> _dbSet;
    private readonly ICurrentTime _timeService;
    private readonly IClaimsService _claimsService;
    
    public GenericRepository(AppDbContext context, ICurrentTime timeService, IClaimsService claimsService)
    {
        _dbSet = context.Set<TEntity>();
        _timeService = timeService;
        _claimsService = claimsService;
    }
    
    public async Task AddAsync(TEntity entity)
    {
        entity.CreationDate = _timeService.GetCurrentTime();
        entity.CreatedBy = _claimsService.GetCurrentUserId;
        await _dbSet.AddAsync(entity);
    }
    
    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public Task<List<TEntity>> GetAllAsync() => _dbSet.ToListAsync();
}
```

**<ENTITY>Repository.cs** — <ENTITY>-specific repository:
```csharp
public class <ENTITY>Repository : GenericRepository<<DOMAIN_ENTITY>>, I<ENTITY>Repository
{
    public <ENTITY>Repository(<SOLUTION_NAME>DbContext context, I<TIME_SERVICE> timeService, I<CONTEXT_SERVICE> <contextService>Service)
        : base(context, timeService, <contextService>Service)
    {
    }
    
    public async Task<<DOMAIN_ENTITY>?> GetBy<UNIQUE_PROPERTY>Async(string <uniqueProperty>)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.<PROPERTY_NAME> == <uniqueProperty>);
    }
}
```

**UnitOfWork.cs** — Coordinates multiple repositories:
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly <SOLUTION_NAME>DbContext _dbContext;
    private readonly I<ENTITY>Repository _<entity>Repository;
    private readonly I<ANOTHER_ENTITY>Repository _<another_entity>Repository;
    
    public UnitOfWork(<SOLUTION_NAME>DbContext dbContext, I<ENTITY>Repository <entity>Repository, I<ANOTHER_ENTITY>Repository <another_entity>Repository)
    {
        _dbContext = dbContext;
        _<entity>Repository = <entity>Repository;
        _<another_entity>Repository = <another_entity>Repository;
    }
    
    public I<ENTITY>Repository <ENTITY>Repository => _<entity>Repository;
    public I<ANOTHER_ENTITY>Repository <ANOTHER_ENTITY>Repository => _<another_entity>Repository;
    
    public async Task<int> SaveChangeAsync() => await _dbContext.SaveChangesAsync();
}
```

**MapperConfigProfile.cs** — AutoMapper configuration:
```csharp
public class MapperConfigProfile : Profile
{
    public MapperConfigProfile()
    {
        CreateMap<CreateProductViewModel, Product>();
        CreateMap<Product, ProductResponseDTO>();
    }
}
```

**DenpendencyInjection.cs** — Register Infrastructure services:
```csharp
public static class DenpendencyInjection
{
    public static IServiceCollection AddInfrastructuresService(
        this IServiceCollection services, 
        string databaseConnection)
    {
        // DbContext
        services.AddDbContext<<SOLUTION_NAME>DbContext>(
            option => option.UseSqlServer(databaseConnection));
        
        // Identity
        services.AddIdentity<<DOMAIN_ENTITY>, IdentityRole>()
            .AddEntityFrameworkStores<<SOLUTION_NAME>DbContext>()
            .AddDefaultTokenProviders();
        
        // Repositories
        services.AddScoped<I<ENTITY>Repository, <ENTITY>Repository>();
        services.AddScoped<I<ANOTHER_ENTITY>Repository, <ANOTHER_ENTITY>Repository>();
        services.AddScoped<I<THIRD_ENTITY>Repository, <THIRD_ENTITY>Repository>();
        
        // Services
        services.AddScoped<I<ENTITY>Service, <ENTITY>Service>();
        services.AddScoped<I<ANOTHER_ENTITY>Service, <ANOTHER_ENTITY>Service>();
        services.AddScoped<I<THIRD_ENTITY>Service, <THIRD_ENTITY>Service>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Singletons
        services.AddSingleton<I<TIME_SERVICE>, <TIME_SERVICE>>();
        
        // AutoMapper
        services.AddAutoMapper(typeof(MapperConfigProfile).Assembly);
        
        return services;
    }
}
```

#### When to Add Files

Add to Infrastructure when:
- **Persistence/Contexts/** — Database context (DbContext, ORM configuration)
- **Repositories/** — Implementation of repository interfaces
- **Services/** — Implementations of external service interfaces (Email, SMS, etc.)
- **Mapper/** — AutoMapper/Mapster profiles
- **ExternalClients/** — HTTP clients wrapping external APIs
- **Settings/** — Configuration option classes for settings binding

#### What NOT to Put Here

- ❌ Interface definitions (those belong in Application)
- ❌ Business logic (that belongs in Domain or Application)
- ❌ Controllers or HTTP handlers
- ❌ ViewModels or API DTOs

#### Dependency Rules

- ✅ Infrastructure **can reference** Application and Domain
- ✅ Infrastructure **implements** contracts defined in Application
- ❌ Infrastructure **cannot force dependencies** on Application or Domain

---

### Solution-Level Files

```
Solution/
├── <PROJECT_NAME>.slnx                    # Solution file (groups projects)
├── Program.cs                           # (In <PROJECT_NAME>.API)
├── Makefile                             # Convenience commands (make run, make build)
├── README.md                            # Project documentation
├── docker-compose.yml                   # (Optional) Local development services
├── Dockerfile                           # (Optional) Container build
├── .env                                 # (Optional) Environment variables
└── .gitignore                           # Git exclusion rules
```

#### Key Files

**Program.cs** — Application entry point and middleware configuration.

**Makefile** — Provides shortcuts:
```makefile
run:
	dotnet run --project ./<PROJECT_NAME>.API/<PROJECT_NAME>.API.csproj

run-https:
	dotnet run --project ./<PROJECT_NAME>.API/<PROJECT_NAME>.API.csproj --launch-profile "https"

build:
	dotnet build ./<PROJECT_NAME>.csproj

swagger:
	open https://localhost:7040/swagger/index.html
```

**<PROJECT_NAME>.API.http** — REST client examples (VS Code REST Client, Rider):
```http
### Get all <entities>
GET http://localhost:5087/api/<entities>

### Create <entity>
POST http://localhost:5087/api/<entities>
Content-Type: application/json

{
  "<propertyName>": "<value>",
  "<propertyDescription>": "<description>",
  "<propertyValue>": 100
}
```

---

## Directory Dependencies Summary

| From Layer | Can Reference | Cannot Reference |
|:-----------|:--------------|:-----------------|
| **API** | Application, Domain | Infrastructure |
| **Application** | Domain | API, Infrastructure |
| **Domain** | None (external lib only) | All others |
| **Infrastructure** | Application, Domain | API |

**Unidirectional Rule:** Always point inward. Outer layers depend on inner layers, never the reverse.

---

## Recommended Workflow for New Projects

1. **Define Domain first** — Model core business entities and rules
2. **Define Application interfaces** — Specify what the system does
3. **Build API controllers** — Create endpoints that call Application services
4. **Implement Infrastructure** — Create repositories and external service adapters
5. **Register DI** — Connect everything in Program.cs and DependencyInjection extension

This ensures clean separation of concerns and makes testing and refactoring easier.

---

## Quick Reference

### Clean Architecture Layers

| Layer | Project Name | Key Folders | Responsibilities |
|-------|--------------|-------------|------------------|
| **Domain** | `{Solution}.Domain` | `Entities/`, `Enums/`, `ValueObjects/`, `Exceptions/` | Business rules, entities, invariants (No frameworks) |
| **Application** | `{Solution}.Application` | `Services/`, `Interfaces/`, `Repositories/`, `ViewModels/`, `Commons/` | Use-cases, orchestration, DTOs, port definitions |
| **Infrastructure** | `{Solution}.Infrastructure` | `Persistence/`, `Repositories/`, `Mapper/`, `DependencyInjection/` | Database, external services, implementations |
| **API** | `{Solution}.API` | `Controllers/`, `Middlewares/`, `Services/`, `Program.cs` | HTTP endpoints, request/response handling |

### Key File Types

| File Type | Purpose | Location | Naming Convention |
|-----------|---------|---------|-------------------|
| `BaseEntity.cs` | Abstract base with audit fields (Id, CreatedBy, etc.) | `Domain/Entities/` | Fixed name |
| `I<ENTITY>Service.cs` | Service port/interface | `Application/Interfaces/` | `I` + entity name + `Service` |
| `<ENTITY>Service.cs` | Service implementation | `Application/Services/` | Entity name + `Service` |
| `I<ENTITY>Repository.cs` | Repository port/interface | `Application/Repositories/` | `I` + entity name + `Repository` |
| `<ENTITY>Repository.cs` | Repository implementation | `Infrastructure/Repositories/` | Entity name + `Repository` |
| `Create<ENTITY>ViewModel.cs` | Input DTO | `Application/ViewModels/` | `Create` + entity name + `ViewModel` |
| `ResponseModel.cs` | Response wrapper | `Application/ViewModels/ResponseModels/` | `ResponseModel` or `<OPERATION>ResponseModel` |
| `Pagination.cs` | Generic pagination container | `Application/Commons/` | Fixed name (generic) |
| `<ENTITY>sController.cs` | HTTP controller (thin) | `API/Controllers/` | Entity name (plural) + `Controller` |
| `GlobalExceptionMiddleware.cs` | Cross-cutting error handling | `API/Middlewares/` | `<CONCERN>Middleware` |
| `<SOLUTION_NAME>DbContext.cs` | Entity Framework context | `Infrastructure/Persistence/Contexts/` | Solution name + `DbContext` |
| `<ENTITY>Config.cs` | FluentAPI entity configuration | `Infrastructure/FluentAPIs/` | Entity name + `Config` |
| `MapperConfigProfile.cs` | AutoMapper profile | `Infrastructure/Mapper/` | Fixed name |
| `DependencyInjection.cs` | DI registration extension | `{Layer}/` (one per layer) | Fixed name per layer |
| `IUnitOfWork.cs` | Unit of Work pattern interface | `Application/` | Fixed name |
| `UnitOfWork.cs` | Unit of Work implementation | `Infrastructure/` | Fixed name |

### Build & Configuration Files

| File | Purpose |
|------|---------|
| `{Solution}.slnx` | Modern XML solution file |
| `Directory.Build.props` | Centralized build properties |
| `Directory.Packages.props` | Central package version management |
| `global.json` | SDK version pinning |
| `NuGet.Config` | Package source configuration |
| `RELEASE_NOTES.md` | Version history (parsed by build) |
| `build.ps1` | Build orchestration script |
| `.config/dotnet-tools.json` | Local .NET tools |
| `Makefile` | Development command shortcuts |
| `{Solution}.API.http` | REST client test examples |

### Dependency Rules (Unidirectional)

```
API → Application → Domain
Infrastructure → Application, Domain
API ↛ Infrastructure (not directly)
Application ↛ API, Infrastructure
Domain ↛ Everything else
```

**Golden Rule:** Outer layers can reference inner layers, but NEVER the reverse.


