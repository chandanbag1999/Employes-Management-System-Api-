# AI Backend Structure Template

## Purpose

This document is a **reusable blueprint** for future backend projects that should follow the **same folder structure, same Clean Architecture layering, same modular design, and same naming discipline** as the current `EmployeeManagementSystemBackend` codebase.

You can use this document in 2 ways:

1. **As a reference blueprint** while manually creating a new project
2. **As a ready-to-paste prompt for any AI model** so it generates code in the same architecture

---

## 1) Architecture Style Used In This Backend

This backend follows a **4-project Clean Architecture** with modular organization.

### Layers

1. **`ProjectName.API`**
   - HTTP entry point
   - Controllers
   - Middleware
   - App startup/configuration
   - API versioning route structure

2. **`ProjectName.Application`**
   - Business logic
   - DTOs
   - Interfaces
   - Services
   - Module-wise use case orchestration

3. **`ProjectName.Domain`**
   - Core entities
   - Enums
   - Shared base classes
   - Pure business model definitions

4. **`ProjectName.Infrastructure`**
   - EF Core / persistence
   - DbContext
   - Fluent configurations
   - Repository implementations
   - External services
   - Background services
   - Seeders
   - Dependency injection wiring

### Dependency Direction

Use this exact dependency rule:

```text
ProjectName.API
 ├── references ProjectName.Application
 ├── references ProjectName.Infrastructure
 └── references ProjectName.Domain

ProjectName.Infrastructure
 ├── references ProjectName.Application
 └── references ProjectName.Domain

ProjectName.Application
 └── references ProjectName.Domain

ProjectName.Domain
 └── references nothing
```

### Core Principle

- **Domain must stay pure**
- **Application contains business rules**
- **Infrastructure contains implementation details**
- **API contains transport concerns only**

---

## 2) Exact Folder Structure Pattern

Use the following structure as the base pattern for every new backend project.

```text
ProjectRoot/
│
├── Docs/
│   ├── PROJECT_BACKEND_DOCUMENTATION.md
│   ├── ModuleA/
│   │   └── ModuleA_Module.md
│   ├── ModuleB/
│   │   └── ModuleB_Module.md
│   └── ModuleC/
│       └── ModuleC_Module.md
│
├── src/
│   ├── ProjectName.Domain/
│   │   ├── Common/
│   │   │   └── BaseEntity.cs
│   │   ├── Entities/
│   │   │   ├── Identity/
│   │   │   ├── ModuleA/
│   │   │   ├── ModuleB/
│   │   │   ├── ModuleC/
│   │   │   └── ModuleD/
│   │   └── Enums/
│   │       ├── UserRole.cs
│   │       ├── StatusEnum.cs
│   │       └── OtherEnum.cs
│   │
│   ├── ProjectName.Application/
│   │   ├── Common/
│   │   │   ├── DTOs/
│   │   │   │   ├── ApiResponse.cs
│   │   │   │   └── PaginatedResult.cs
│   │   │   └── Interfaces/
│   │   │       ├── IJwtService.cs
│   │   │       └── IEmailService.cs
│   │   └── Modules/
│   │       ├── Identity/
│   │       │   ├── DTOs/
│   │       │   ├── Interfaces/
│   │       │   └── Services/
│   │       ├── ModuleA/
│   │       │   ├── DTOs/
│   │       │   ├── Interfaces/
│   │       │   └── Services/
│   │       ├── ModuleB/
│   │       │   ├── DTOs/
│   │       │   ├── Interfaces/
│   │       │   └── Services/
│   │       ├── ModuleC/
│   │       │   ├── DTOs/
│   │       │   ├── Interfaces/
│   │       │   └── Services/
│   │       ├── Dashboard/
│   │       │   ├── DTOs/
│   │       │   └── Interfaces/
│   │       └── Reports/
│   │           ├── DTOs/
│   │           └── Interfaces/
│   │
│   ├── ProjectName.Infrastructure/
│   │   ├── DependencyInjection.cs
│   │   ├── BackgroundServices/
│   │   ├── Migrations/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/
│   │   ├── Repositories/
│   │   │   ├── GenericRepository.cs
│   │   │   ├── ModuleARepository.cs
│   │   │   ├── ModuleBRepository.cs
│   │   │   └── ModuleCRepository.cs
│   │   ├── Seeders/
│   │   ├── Services/
│   │   │   ├── JwtService.cs
│   │   │   ├── Dashboard/
│   │   │   └── Reports/
│   │   └── UnitOfWork/
│   │       └── UnitOfWork.cs
│   │
│   └── ProjectName.API/
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       ├── Program.cs
│       ├── Controllers/
│       │   └── v1/
│       │       ├── AuthController.cs
│       │       ├── UsersController.cs
│       │       ├── ModuleAController.cs
│       │       ├── ModuleBController.cs
│       │       ├── DashboardController.cs
│       │       ├── ReportsController.cs
│       │       └── HealthController.cs
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs
│       └── Properties/
│           ├── launchSettings.json
│           └── PublishProfiles/
│
├── tests/
│   ├── ProjectName.UnitTests/
│   │   ├── GlobalUsings.cs
│   │   ├── Helpers/
│   │   └── Modules/
│   │       ├── Identity/
│   │       ├── ModuleA/
│   │       ├── ModuleB/
│   │       └── ModuleC/
│   └── ProjectName.IntegrationTests/
│
├── .gitignore
└── ProjectName.slnx
```

---

## 3) Naming Conventions You Must Keep Exactly

### Project Names

- Use project names in this format:
  - `ProjectName.API`
  - `ProjectName.Application`
  - `ProjectName.Domain`
  - `ProjectName.Infrastructure`
  - `ProjectName.UnitTests`
  - `ProjectName.IntegrationTests`

### Main Technical File Names

- `Program.cs`
- `DependencyInjection.cs`
- `AppDbContext.cs`
- `BaseEntity.cs`
- `ExceptionMiddleware.cs`
- `ApiResponse.cs`
- `PaginatedResult.cs`

### Module Naming Style

- Keep business features grouped as **modules**
- Each module gets its own folder under `Application/Modules`
- Domain entities should also be grouped by business area
- API should expose one controller per main module

Example module naming pattern:

- `Identity`
- `Organization`
- `Employees`
- `Attendance`
- `Leave`
- `Payroll`
- `Performance`
- `Dashboard`
- `Reports`

### Interface Naming

- Service interfaces: `IEmployeeService`, `ILeaveService`, `IReportService`
- Repository interfaces: `IEmployeeRepository`, `ILeaveRepository`
- Shared service interfaces: `IJwtService`, `IEmailService`

### Service Naming

- `EmployeeService`
- `LeaveService`
- `DepartmentService`
- `DashboardService`
- `ReportService`

### Repository Naming

- `EmployeeRepository`
- `LeaveRepository`
- `DepartmentRepository`
- `GenericRepository`

### Controller Naming

- `EmployeesController`
- `LeaveController`
- `PayrollController`
- `ReportsController`
- `HealthController`

### DTO Naming

Use intent-based DTO names such as:

- `CreateEmployeeDto`
- `UpdateEmployeeDto`
- `EmployeeResponseDto`
- `EmployeeFilterDto`
- `RunPayrollDto`
- `ApplyLeaveDto`
- `LoginRequestDto`

---

## 4) Layer Responsibilities

### `ProjectName.Domain`

Put only:

- entities
- enums
- base abstractions
- domain-only logic if truly pure

Do **not** put:

- EF Core DbContext
- repository implementations
- controller logic
- HTTP concerns
- DTOs

### `ProjectName.Application`

Put only:

- business use cases
- service interfaces
- repository interfaces (if used from application)
- DTOs
- validation or orchestration logic

Do **not** put:

- EF Core implementation details
- controller classes
- web middleware

### `ProjectName.Infrastructure`

Put only:

- `AppDbContext`
- entity configurations
- migrations
- repositories
- external providers
- email/jwt implementations
- background jobs
- seeding logic
- dependency injection registrations

### `ProjectName.API`

Put only:

- controllers
- middleware
- startup pipeline
- auth/authorization setup
- appsettings
- OpenAPI/Swagger/Scalar wiring

Controllers must stay thin:

- validate request model state
- call application service
- return standardized response wrapper

---

## 5) Module Folder Rule

For every business module, follow this pattern:

```text
ProjectName.Application/Modules/ModuleName/
├── DTOs/
├── Interfaces/
└── Services/
```

### What goes where?

#### `DTOs/`
- request DTOs
- response DTOs
- filter DTOs
- summary DTOs
- action DTOs

#### `Interfaces/`
- service contracts
- repository contracts if the module exposes them from Application

#### `Services/`
- business logic implementation
- mapping to DTOs
- orchestration across repositories/services

### Domain side for the same module

```text
ProjectName.Domain/Entities/ModuleName/
└── EntityName.cs
```

### API side for the same module

```text
ProjectName.API/Controllers/v1/
└── ModuleNameController.cs
```

### Infrastructure side for the same module

```text
ProjectName.Infrastructure/Repositories/
└── ModuleNameRepository.cs
```

and if needed:

```text
ProjectName.Infrastructure/Persistence/Configurations/
└── EntityNameConfiguration.cs
```

---

## 6) Current Codebase Patterns That Must Be Preserved

These patterns are visible in the current backend and should remain the default template for future projects:

1. **Versioned API controllers**
   - controllers inside `Controllers/v1/`

2. **Central DI registration**
   - all service/repository registrations in `Infrastructure/DependencyInjection.cs`

3. **Single EF Core DbContext**
   - `Persistence/AppDbContext.cs`

4. **Entity configurations separated from entities**
   - Fluent API files in `Persistence/Configurations/`

5. **Standardized API responses**
   - shared `ApiResponse<T>` wrapper

6. **Pagination wrapper**
   - shared `PaginatedResult<T>`

7. **Thin controllers + service-driven business logic**

8. **Repository abstraction**
   - interfaces in Application
   - implementations in Infrastructure

9. **Background jobs in Infrastructure**
   - ex: cleanup schedulers, automation tasks

10. **Seeders in Infrastructure**
   - initial admin/system bootstrap logic

11. **Module-wise docs under `Docs/`**

12. **Unit tests grouped by module**
   - `tests/ProjectName.UnitTests/Modules/...`

---

## 7) Rules For Adding A New Module

Whenever a new module is introduced, create all corresponding pieces in the same architecture.

### Example

If a new module is `Assets`, create:

```text
src/
├── ProjectName.Domain/
│   └── Entities/Assets/
│       └── Asset.cs
│
├── ProjectName.Application/
│   └── Modules/Assets/
│       ├── DTOs/
│       ├── Interfaces/
│       └── Services/
│
├── ProjectName.Infrastructure/
│   ├── Repositories/
│   │   └── AssetRepository.cs
│   └── Persistence/Configurations/
│       └── AssetConfiguration.cs
│
└── ProjectName.API/
    └── Controllers/v1/
        └── AssetsController.cs
```

Also update:

- `AppDbContext.cs`
- `DependencyInjection.cs`
- docs under `Docs/Assets/`
- unit tests under `tests/ProjectName.UnitTests/Modules/Assets/`

---

## 8) Ready-To-Paste Prompt For Any AI Model

Copy the full prompt below and give it to any AI model when you want a new backend project in the same structure.

```text
Create a backend project using the exact same architecture and folder discipline as my reference backend.

Follow these rules strictly:

1. Use a 4-project Clean Architecture solution:
   - ProjectName.API
   - ProjectName.Application
   - ProjectName.Domain
   - ProjectName.Infrastructure

2. Also create:
   - tests/ProjectName.UnitTests
   - tests/ProjectName.IntegrationTests
   - Docs/ folder for project and module documentation

3. Keep dependency direction exactly like this:
   - API references Application, Infrastructure, Domain
   - Infrastructure references Application and Domain
   - Application references Domain
   - Domain references nothing

4. In Domain, create only:
   - Common/BaseEntity.cs
   - Entities/<ModuleName>/...
   - Enums/...

5. In Application, create:
   - Common/DTOs/ApiResponse.cs
   - Common/DTOs/PaginatedResult.cs
   - Common/Interfaces/IJwtService.cs
   - Common/Interfaces/IEmailService.cs if needed
   - Modules/<ModuleName>/DTOs
   - Modules/<ModuleName>/Interfaces
   - Modules/<ModuleName>/Services

6. In Infrastructure, create:
   - DependencyInjection.cs
   - Persistence/AppDbContext.cs
   - Persistence/Configurations/
   - Repositories/
   - Services/
   - BackgroundServices/
   - Seeders/
   - UnitOfWork/
   - Migrations/

7. In API, create:
   - Program.cs
   - Controllers/v1/
   - Middleware/ExceptionMiddleware.cs
   - appsettings.json
   - appsettings.Development.json
   - appsettings.Production.json
   - Properties/launchSettings.json

8. Keep the code modular. For each business module, create matching folders across layers where applicable.

9. Each module in Application must follow this structure:
   - Modules/<ModuleName>/DTOs
   - Modules/<ModuleName>/Interfaces
   - Modules/<ModuleName>/Services

10. Keep controllers thin, business logic in services, persistence in repositories, entities in domain.

11. Use naming conventions like:
   - EntityName.cs
   - EntityNameConfiguration.cs
   - IEntityNameService.cs
   - EntityNameService.cs
   - IEntityNameRepository.cs
   - EntityNameRepository.cs
   - EntityNamesController.cs
   - CreateEntityDto / UpdateEntityDto / EntityResponseDto / EntityFilterDto

12. Place all API controllers under Controllers/v1.

13. Put all dependency injection registrations in Infrastructure/DependencyInjection.cs.

14. Put DbSet declarations in Persistence/AppDbContext.cs and entity configurations in Persistence/Configurations.

15. Create unit tests module-wise under tests/ProjectName.UnitTests/Modules.

16. Also generate the final folder tree in the output before generating code.

17. Do not collapse everything into a single project. Do not break the layer boundaries.

18. Use the following root structure exactly:

ProjectRoot/
├── Docs/
├── src/
│   ├── ProjectName.API/
│   ├── ProjectName.Application/
│   ├── ProjectName.Domain/
│   └── ProjectName.Infrastructure/
├── tests/
│   ├── ProjectName.UnitTests/
│   └── ProjectName.IntegrationTests/
├── .gitignore
└── ProjectName.slnx

19. Before implementing any feature, first scaffold the complete structure.

20. The implementation must look like an enterprise modular .NET backend with Clean Architecture, EF Core, DI, versioned controllers, DTOs, repositories, services, middleware, and tests.
```

---

## 9) Short Prompt Version

If you want a smaller prompt, use this:

```text
Build my new backend in the same modular 4-layer Clean Architecture as my existing .NET backend.

Required projects:
- ProjectName.API
- ProjectName.Application
- ProjectName.Domain
- ProjectName.Infrastructure
- tests/ProjectName.UnitTests
- tests/ProjectName.IntegrationTests

Rules:
- Domain = entities, enums, base classes only
- Application = DTOs, interfaces, services, module-wise business logic
- Infrastructure = DbContext, configurations, repositories, migrations, external services, DI, seeders, background jobs
- API = Program.cs, Controllers/v1, middleware, appsettings
- Keep controllers thin
- Keep business logic in services
- Keep repository implementations in Infrastructure
- Keep interfaces in Application
- Use Common/DTOs/ApiResponse.cs and PaginatedResult.cs
- Use AppDbContext + DependencyInjection.cs
- Use versioned controllers under Controllers/v1
- Organize features module-wise across layers
- Generate the folder tree first, then generate the code
```

---

## 10) Best Practice Instruction For Future Use

Whenever you ask an AI to generate a new backend from this template, also provide these 3 values:

1. **Project name**
   - example: `InventoryManagementSystemBackend`

2. **Module list**
   - example: `Identity, Products, Categories, Suppliers, Orders, Dashboard, Reports`

3. **Tech stack constraints**
   - example: `.NET 10, EF Core, PostgreSQL, JWT, OpenAPI`

### Example Usage

```text
Use the attached architecture template.
Create a new backend named InventoryManagementSystemBackend.
Use the same folder structure and same architecture rules.

Modules:
- Identity
- Products
- Categories
- Suppliers
- Inventory
- Orders
- Dashboard
- Reports

Tech stack:
- .NET 10
- EF Core
- PostgreSQL
- JWT Bearer Authentication
- OpenAPI/Scalar
```

---

## 11) Final Recommendation

For best results, always instruct the AI to do the work in this order:

1. generate root folder tree
2. generate solution + project references
3. generate Domain layer
4. generate Application layer
5. generate Infrastructure layer
6. generate API layer
7. wire dependency injection
8. generate docs
9. generate tests

This order matches the structure and architectural behavior of your current backend.