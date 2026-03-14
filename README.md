# EmployesManagementSystemApi (Backend)

Production-oriented ASP.NET Core Web API for employee and department management with JWT authentication, role-based authorization, and PostgreSQL persistence.

---

## Highlights

- Layered architecture: **Controller → Service → Repository → EF Core**
- JWT authentication + role-based access (`Admin`, `User`)
- BCrypt password hashing for secure credential storage
- DTO-driven request/response contracts with validation attributes
- OpenAPI + Scalar documentation in production
- Neon PostgreSQL integration via Npgsql + EF Core

---

## Tech Stack

| Area | Technology |
|---|---|
| Runtime | .NET `net10.0` |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core |
| Database | PostgreSQL (Neon) |
| Auth | JWT Bearer + BCrypt |
| API Docs | Microsoft OpenAPI + Scalar.AspNetCore |

---

## Architecture

```text
Controllers
  ├─ AuthController
  ├─ EmployeeController
  ├─ DepartmentController
  └─ HealthController
      ↓
Services (business rules + mapping)
      ↓
Repositories (data access abstractions)
      ↓
AppDbContext (EF Core)
      ↓
PostgreSQL (Neon)
```

Key folders:

```text
EmployesManagementSystemApi/
├── Controllers/
├── Services/
├── Repositories/
├── DTOs/
├── Data/
├── Models/
├── Migrations/
├── Program.cs
└── appsettings*.json
```

---

## Domain Model Overview

- **AppUser**: application identity (`UserName`, `Email`, `PasswordHash`, `Role`)
- **Department**: department master entity
- **Employee**: employee profile linked to one department (`DepartmentId`)

Important data constraints (via `AppDbContext`):
- Unique `AppUser.Email`
- Unique `Employee.Email`
- Unique `Department.Name`
- Employee → Department delete behavior is restricted (prevents invalid cascade)

---

## Authentication & Authorization

### Authentication

- `/api/auth/register` creates user and returns JWT
- `/api/auth/login` validates credentials and returns JWT
- Password handling uses BCrypt hash/verify

### Authorization

- Most business endpoints require authentication (`[Authorize]`)
- Create/Update/Delete operations are restricted to Admin role on employee/department controllers
- JWT contains standard claims including user id, email, username, and role

---

## API Endpoint Groups

### Auth (Anonymous)
- `POST /api/auth/register`
- `POST /api/auth/login`

### Department (Authorized)
- `GET /api/department`
- `GET /api/department/{id}`
- `POST /api/department` *(Admin)*
- `PUT /api/department/{id}` *(Admin)*
- `DELETE /api/department/{id}` *(Admin)*

### Employee (Authorized)
- `GET /api/employee`
- `GET /api/employee/{id}`
- `GET /api/employee/department/{departmentId}`
- `POST /api/employee` *(Admin)*
- `PUT /api/employee/{id}` *(Admin)*
- `DELETE /api/employee/{id}` *(Admin)*

### Health
- `GET /api/health/ping`

---

## API Documentation (Live)

- Scalar UI: `https://emp-mgmt-api.runasp.net/scalar/v1`
- OpenAPI JSON: `https://emp-mgmt-api.runasp.net/openapi/v1.json`

---

## Configuration

Use `appsettings.Production.json` for production values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_NEON_CONNECTION_STRING"
  },
  "JwtSettings": {
    "Secret": "YOUR_32+_CHAR_SECRET",
    "Issuer": "EmployeesManagementApi",
    "Audience": "EmployeesManagementClient",
    "ExpiryInMinutes": 60
  }
}
```

> `JwtSettings:Secret` must be present and at least 32 characters.

---

## Local Development Setup

### Prerequisites

- .NET SDK compatible with `net10.0`
- PostgreSQL/Neon connection string
- `dotnet-ef` CLI tool

### Run locally

```bash
cd EmployesManagementSystemApi
dotnet restore
dotnet ef database update
dotnet run
```

Verify health:

`GET http://localhost:<port>/api/health/ping`

---

## Deployment Notes

- Backend is deployed on RunASP/MonsterASP
- Production settings should be supplied securely (file or environment variables)
- OpenAPI requires both:
  - `builder.Services.AddOpenApi();`
  - `app.MapOpenApi();`

---

## Enterprise Considerations (Next Iteration)

- Add refresh token support and token revocation
- Add centralized exception middleware + problem details responses
- Add rate limiting and API throttling
- Add unit/integration test suites
- Add CI/CD pipeline with quality gates and security scans
