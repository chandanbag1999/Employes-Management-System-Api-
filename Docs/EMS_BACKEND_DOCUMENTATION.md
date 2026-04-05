# 📘 Employee Management System — Backend Documentation

> **Living Document** — Har update, bug fix, aur feature addition ke saath yeh file update hoti rahegi.

---

## 📋 Table of Contents

1. [Project Overview](#1-project-overview)
2. [Problem Statement — Kya Problem Solve Kar Rahe Hain](#2-problem-statement)
3. [Tech Stack — Kya Kyon Chuna](#3-tech-stack)
4. [Architecture Decision — Clean Architecture Kyon](#4-architecture-decision)
5. [Folder Structure](#5-folder-structure)
6. [Layer-by-Layer Explanation](#6-layer-by-layer-explanation)
7. [Database Design](#7-database-design)
8. [API Modules — Kya Kya Bana](#8-api-modules)
9. [Security Architecture](#9-security-architecture)
10. [Request Flow — Ek Request Ka Poora Safar](#10-request-flow)
11. [Design Patterns Used](#11-design-patterns-used)
12. [Performance & Scalability](#12-performance--scalability)
13. [API Endpoints Reference](#13-api-endpoints-reference)
14. [Capacity & Load Analysis](#14-capacity--load-analysis)
15. [Known Limitations & Future Improvements](#15-known-limitations--future-improvements)
16. [Changelog — Version History](#16-changelog--version-history)
17. [Code Reference Index](#17-code-reference-index)

---

## 1. Project Overview

| Field | Detail |
|---|---|
| **Project Name** | Employee Management System API (EMS) |
| **Version** | 1.0.0 |
| **Runtime** | .NET 10 |
| **Architecture** | Clean Architecture (4-Project Solution) |
| **Database** | PostgreSQL (Neon Cloud) |
| **Auth** | JWT Bearer + BCrypt |
| **API Docs** | Scalar UI + OpenAPI |
| **Port (Dev)** | http://localhost:5185 |
| **Scalar UI** | http://localhost:5185/scalar/v1 |

### Ek Line Mein Kya Hai Yeh?

> Enterprise-grade Employee Management REST API jo companies ke HR departments ke liye banaya gaya hai — attendance track karne se lekar payroll run karne tak sab kuch ek jagah.

---

## 2. Problem Statement

### Kya Problem Solve Kar Rahe Hain?

**Real companies mein HR ka kaam bohot scattered hota hai:**

```
❌ Attendance Excel mein hoti hai
❌ Leave applications WhatsApp pe aati hain
❌ Salary calculation manually hoti hai
❌ Performance reviews paper pe hote hain
❌ Employee data 10 alag files mein bichri hoti hai
❌ Koi role-based access nahi — sab kuch sab dekh sakte hain
```

**EMS in sabko solve karta hai:**

```
✅ Ek centralized system — sab kuch ek jagah
✅ Role-based access — Admin, HR, Manager, Employee
✅ Auto attendance tracking — Clock In / Clock Out
✅ Leave management — Apply, Approve, Balance track
✅ Payroll automation — Salary calculate, PF deduct, Payslip generate
✅ Performance management — Goals set, Reviews conduct
✅ Real-time dashboard — HR ka overview ek click mein
```

### Target Users (Kaun Use Karega?)

```
1. Super Admin   → Company owner / IT Admin
2. HR Admin      → HR Manager — full access
3. Manager       → Department head — team dekh sakta hai
4. Employee      → Self-service — apna data dekh sakta hai
```

### Business Scale

- **Small companies**: 10-100 employees
- **Medium companies**: 100-1000 employees
- **Current capacity**: ~5,000 concurrent users (horizontal scaling ke saath)

---

## 3. Tech Stack — Kya Kyon Chuna

### Runtime: .NET 10

```
Kyon chose kiya?
✅ Latest LTS version — long term support
✅ Best performance among server frameworks (TechEmpower benchmarks)
✅ Native async/await — high concurrency ke liye perfect
✅ Cross-platform — Windows, Linux, Mac pe run hota hai
✅ Memory efficient — low RAM usage
```

### Database: PostgreSQL (Neon)

```
Kyon chose kiya?
✅ ACID compliant — data consistency guarantee
✅ Best-in-class JSON support
✅ Advanced indexing — queries fast hoti hain
✅ Free tier available — development ke liye
✅ Neon = serverless PostgreSQL — auto-scale, connection pooling built-in
✅ MySQL se zyada reliable for complex relationships (Employee → Department)

Alternatives consider kiye the:
❌ MySQL — PostgreSQL jitna feature-rich nahi
❌ SQL Server — paid, heavy
❌ MongoDB — relational data ke liye inappropriate
```

### ORM: Entity Framework Core 10

```
Kyon chose kiya?
✅ Code-first approach — C# se DB schema banta hai
✅ Migrations — version controlled schema changes
✅ LINQ queries — type-safe, compile-time errors
✅ Fluent API — clean configuration, Domain model clean rehta hai
✅ Soft delete support — IsDeleted flag easily
✅ Query filters — global WHERE clauses (IsDeleted = false)

Alternatives:
❌ Dapper — manual SQL likhna padta, error-prone
❌ NHibernate — complex configuration
```

### Auth: JWT + BCrypt

```
JWT (JSON Web Token):
✅ Stateless — server pe session store nahi karna
✅ Scalable — multiple servers pe kaam karta hai
✅ Claims based — role, email, id sab token mein
✅ Standard — har frontend/mobile app support karta hai

BCrypt:
✅ Adaptive hashing — computationally expensive by design
✅ Salt built-in — rainbow table attacks prevent
✅ Industry standard for password hashing
```

### API Docs: Scalar + OpenAPI

```
Kyon Scalar chose kiya?
✅ Beautiful UI — Swagger se better experience
✅ Built-in request testing
✅ Purple theme (custom)
✅ .NET 10 ke saath native integration
```

---

## 4. Architecture Decision — Clean Architecture Kyon

### Sochne Ki Baat

> Ek simple project mein sab kuch ek jagah rakh sakte the. Toh 4 alag projects kyon?

### Clean Architecture Ka Core Idea

```
┌─────────────────────────────────────────────────────────┐
│  EMS.API          (HTTP — Controllers, Middleware)       │
│  ↓ depends on ↓                                          │
│  EMS.Application  (Business Rules — Services, DTOs)      │
│  ↓ depends on ↓                                          │
│  EMS.Domain       (Core Entities — Models, Enums)        │
│  ↑ referenced by ↑                                       │
│  EMS.Infrastructure (DB, External Services, Repos)       │
└─────────────────────────────────────────────────────────┘

Rule: Dependency sirf inward jaati hai.
Domain kisi pe depend nahi karta.
Application sirf Domain pe depend karta hai.
```

### Practical Fayde

```
1. DATABASE CHANGE?
   PostgreSQL → MySQL karna ho? Sirf Infrastructure badlo.
   Business logic (Application) touch nahi hoga.

2. TESTING?
   Services ko in-memory mock se test kar sakte hain.
   Real DB ki zaroorat nahi unit tests mein.

3. TEAM WORK?
   Alag developer alag layer pe kaam kar sakta hai.
   Conflicts kam hote hain.

4. SCALABILITY?
   Kal agar microservices banana ho?
   Har module already isolated hai.
```

### Tradeoff (Honest Assessment)

```
Overhead:
- Zyada files hain
- Simple feature ke liye bhi 4-5 files touch karni padti hain
- Learning curve hai

Lekin worth it hai because:
- Yeh project grow karega
- Job interviews mein Clean Architecture = 10/10 impression
- Industry standard hai (SAP, Darwinbox, etc.)
```

---

## 5. Folder Structure

```
EmployeeManagementSystemBackend/
│
├── src/
│   ├── EMS.Domain/                    ← Layer 1: Entities + Enums (kisi pe depend nahi)
│   │   ├── Common/
│   │   │   └── BaseEntity.cs          ← Id, CreatedAt, UpdatedAt, IsDeleted
│   │   ├── Entities/
│   │   │   ├── Identity/
│   │   │   │   └── AppUser.cs
│   │   │   ├── Organization/
│   │   │   │   ├── Department.cs
│   │   │   │   └── Designation.cs
│   │   │   ├── Employee/
│   │   │   │   └── EmployeeProfile.cs
│   │   │   ├── Attendance/
│   │   │   │   └── AttendanceRecord.cs
│   │   │   ├── Leave/
│   │   │   │   ├── LeaveType.cs
│   │   │   │   └── LeaveApplication.cs
│   │   │   ├── Payroll/
│   │   │   │   ├── SalaryStructure.cs
│   │   │   │   └── PayrollRecord.cs
│   │   │   └── Performance/
│   │   │       ├── Goal.cs
│   │   │       └── PerformanceReview.cs
│   │   └── Enums/
│   │       ├── UserRole.cs             ← SuperAdmin, HRAdmin, Manager, Employee
│   │       ├── EmploymentStatus.cs     ← Active, OnProbation, Resigned, Terminated
│   │       ├── Gender.cs
│   │       ├── LeaveStatus.cs          ← Pending, Approved, Rejected, Cancelled
│   │       └── AttendanceStatus.cs     ← Present, Absent, HalfDay, Holiday, OnLeave
│   │
│   ├── EMS.Application/               ← Layer 2: Business Logic (sirf Domain pe depend)
│   │   ├── Common/
│   │   │   ├── DTOs/
│   │   │   │   ├── ApiResponse.cs      ← Consistent API response wrapper
│   │   │   │   └── PaginatedResult.cs  ← Pagination wrapper
│   │   │   └── Interfaces/
│   │   │       └── IJwtService.cs
│   │   └── Modules/                   ← Har module apna ek folder
│   │       ├── Identity/              ← AuthService, UserService, RefreshToken
│   │       │   ├── DTOs/              (Register, Login, Refresh, UserResponse, etc.)
│   │       │   ├── Interfaces/        (IAuthService, IUserService, IRefreshTokenRepository)
│   │       │   └── Services/          (AuthService, UserService)
│   │       ├── Organization/          ← DepartmentService, DesignationService
│   │       │   ├── DTOs/              (Department, Designation DTOs)
│   │       │   ├── Interfaces/        (IDepartmentService, IDesignationService)
│   │       │   └── Services/          (DepartmentService, DesignationService)
│   │       ├── Employees/             ← EmployeeService
│   │       │   ├── DTOs/              (Create, Update, Filter, Response)
│   │       │   ├── Interfaces/        (IEmployeeService, IEmployeeRepository)
│   │       │   └── Services/          (EmployeeService)
│   │       ├── Attendance/            ← AttendanceService
│   │       │   ├── DTOs/              (ClockIn, ClockOut, Manual, Filter, Summary)
│   │       │   ├── Interfaces/        (IAttendanceService, IAttendanceRepository)
│   │       │   └── Services/          (AttendanceService)
│   │       ├── Leave/                 ← LeaveService
│   │       │   ├── DTOs/              (Apply, Action, Balance, Filter, Types)
│   │       │   ├── Interfaces/        (ILeaveService, ILeaveRepository)
│   │       │   └── Services/          (LeaveService)
│   │       ├── Payroll/               ← PayrollService
│   │       │   ├── DTOs/              (SalaryStructure, RunPayroll, Record)
│   │       │   ├── Interfaces/        (IPayrollService, IPayrollRepository)
│   │       │   └── Services/          (PayrollService)
│   │       ├── Performance/           ← PerformanceService
│   │       │   ├── DTOs/              (Goal, Review, Summary)
│   │       │   ├── Interfaces/        (IPerformanceService, IPerformanceRepository)
│   │       │   └── Services/          (PerformanceService)
│   │       ├── Dashboard/             ← DashboardService
│   │       │   ├── DTOs/              (Stats, Headcount, Activities)
│   │       │   └── Interfaces/        (IDashboardService)
│   │       └── Reports/               ← ReportService
│   │           ├── DTOs/              (Attendance, Payroll, Headcount Reports)
│   │           └── Interfaces/        (IReportService)
│   │
│   ├── EMS.Infrastructure/            ← Layer 3: DB, Repos, External Services
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs       ← EF Core DbContext
│   │   │   ├── Configurations/        ← EF Core Fluent API — table schemas
│   │   │   │   └── (Configuration files for each entity)
│   │   │   └── Migrations/           ← Auto-generated DB migrations
│   │   ├── Repositories/              ← DB queries implementation
│   │   │   ├── GenericRepository.cs   ← Base repository
│   │   │   ├── AuthRepository.cs      ← Auth data access
│   │   │   ├── RefreshTokenRepository.cs ← Refresh token operations
│   │   │   ├── EmployeeRepository.cs  ← Employee data access
│   │   │   ├── AttendanceRepository.cs ← Attendance data access
│   │   │   ├── LeaveRepository.cs     ← Leave data access
│   │   │   ├── PayrollRepository.cs   ← Payroll data access
│   │   │   ├── PerformanceRepository.cs ← Performance data access
│   │   │   ├── DepartmentRepository.cs ← Department data access
│   │   │   └── DesignationRepository.cs ← Designation data access
│   │   ├── Services/
│   │   │   ├── JwtService.cs          ← JWT token generation/validation
│   │   │   ├── Dashboard/
│   │   │   │   └── DashboardService.cs ← Dashboard analytics
│   │   │   └── Reports/
│   │   │       └── ReportService.cs   ← Report generation
│   │   ├── BackgroundServices/
│   │   │   └── TokenCleanupService.cs  ← Background token cleanup
│   │   ├── Seeders/
│   │   │   └── SuperAdminSeeder.cs    ← Auto seed SuperAdmin
│   │   ├── UnitOfWork/
│   │   │   └── UnitOfWork.cs         ← Transaction management
│   │   └── DependencyInjection.cs     ← All DI registrations
│   │
│   └── EMS.API/                       ← Layer 4: HTTP Entry Point
│       ├── Controllers/v1/            ← Versioned endpoints
│       │   ├── AuthController.cs       ← POST register, login, refresh, logout
│       │   ├── UsersController.cs       ← GET users, PATCH role, deactivate
│       │   ├── DepartmentsController.cs ← CRUD departments
│       │   ├── DesignationsController.cs ← CRUD + restore/purge designations
│       │   ├── EmployeesController.cs   ← CRUD employees + filters
│       │   ├── AttendanceController.cs  ← Clock in/out, manual, summary
│       │   ├── LeaveController.cs       ← Apply, approve/reject, cancel, balance
│       │   ├── PayrollController.cs     ← Salary structure, run, payslip
│       │   ├── PerformanceController.cs ← Goals, reviews, summary
│       │   ├── DashboardController.cs  ← Stats, headcount, activities
│       │   ├── ReportsController.cs     ← Attendance, payroll, headcount reports
│       │   └── HealthController.cs     ← Ping, db connection check
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs  ← Global error handling
│       ├── Program.cs                  ← App entry point
│       ├── Properties/
│       │   ├── launchSettings.json
│       │   └── PublishProfiles/
│       └── appsettings.json
│
└── tests/
    ├── EMS.UnitTests/                 ← xUnit + Moq + FluentAssertions
    └── EMS.IntegrationTests/          ← (Future)
```

---

## 6. Layer-by-Layer Explanation

### Layer 1: EMS.Domain

**Kya hai:** Pure business entities — koi DB logic nahi, koi HTTP logic nahi.

**📁 Source File:** [`EMS.Domain/Common/BaseEntity.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Common/BaseEntity.cs)

**BaseEntity** — Har entity ka base:
```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete
}
```

**All Domain Entities:**
| Entity | File Path |
|--------|-----------|
| AppUser | [`EMS.Domain/Entities/Identity/AppUser.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Identity/AppUser.cs) |
| RefreshToken | [`EMS.Domain/Entities/Identity/RefreshToken.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Identity/RefreshToken.cs) |
| EmployeeProfile | [`EMS.Domain/Entities/Employee/EmployeeProfile.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Employee/EmployeeProfile.cs) |
| Department | [`EMS.Domain/Entities/Organization/Department.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Organization/Department.cs) |
| Designation | [`EMS.Domain/Entities/Organization/Designation.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Organization/Designation.cs) |
| AttendanceRecord | [`EMS.Domain/Entities/Attendance/AttendanceRecord.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Attendance/AttendanceRecord.cs) |
| LeaveApplication | [`EMS.Domain/Entities/Leave/LeaveApplication.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Leave/LeaveApplication.cs) |
| LeaveType | [`EMS.Domain/Entities/Leave/LeaveType.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Leave/LeaveType.cs) |
| PayrollRecord | [`EMS.Domain/Entities/Payroll/PayrollRecord.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Payroll/PayrollRecord.cs) |
| SalaryStructure | [`EMS.Domain/Entities/Payroll/SalaryStructure.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Payroll/SalaryStructure.cs) |
| Goal | [`EMS.Domain/Entities/Performance/Goal.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Performance/Goal.cs) |
| PerformanceReview | [`EMS.Domain/Entities/Performance/PerformanceReview.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Entities/Performance/PerformanceReview.cs) |

**All Enums:**
| Enum | File Path |
|------|-----------|
| UserRole | [`EMS.Domain/Enums/UserRole.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Enums/UserRole.cs) |
| EmploymentStatus | [`EMS.Domain/Enums/EmploymentStatus.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Enums/EmploymentStatus.cs) |
| Gender | [`EMS.Domain/Enums/Gender.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Enums/Gender.cs) |
| LeaveStatus | [`EMS.Domain/Enums/LeaveStatus.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Enums/LeaveStatus.cs) |
| AttendanceStatus | [`EMS.Domain/Enums/AttendanceStatus.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Domain/Enums/AttendanceStatus.cs) |

**All Enums (Actual Values):**
| Enum | Values |
|------|--------|
| UserRole | SuperAdmin=1, HRAdmin=2, Manager=3, Employee=4 |
| EmploymentStatus | Active=1, Inactive=2, OnProbation=3, Resigned=4, Terminated=5 |
| Gender | Male=1, Female=2, Other=3 |
| LeaveStatus | (see LeaveStatus.cs) |
| AttendanceStatus | Present=1, Absent=2, HalfDay=3, Holiday=4, WeekOff=5, OnLeave=6 |

**Kyon Soft Delete?**
- Real companies mein data permanently delete nahi hoti
- Audit trail maintain hoti hai
- Undo possible hota hai
- EF Core Query Filter lagane se har query mein automatic `WHERE IsDeleted = false`

---

### Layer 2: EMS.Application

**Kya hai:** Business rules, validation logic, data transformation.

**📁 Source Files:**
- [`EMS.Application/Common/DTOs/ApiResponse.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Application/Common/DTOs/ApiResponse.cs)
- [`EMS.Application/Common/DTOs/PaginatedResult.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Application/Common/DTOs/PaginatedResult.cs)

**ApiResponse Pattern** — Consistent response structure:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}
```

**Kyon yeh pattern?**
- Frontend developer ko predictable response milta hai
- Error handling easy hoti hai
- Success/failure ek flag se pata chalta hai

**PaginatedResult Pattern:**
```csharp
public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}
```

---

### Layer 3: EMS.Infrastructure

**Kya hai:** Database ke saath interaction, JWT generation, external services.

**📁 Source Files:**
| File | Purpose |
|------|---------|
| [`EMS.Infrastructure/DependencyInjection.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/DependencyInjection.cs) | All DI registrations |
| [`EMS.Infrastructure/Persistence/AppDbContext.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Persistence/AppDbContext.cs) | EF Core DbContext |
| [`EMS.Infrastructure/Seeders/SuperAdminSeeder.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Seeders/SuperAdminSeeder.cs) | Auto seeds SuperAdmin |
| [`EMS.Infrastructure/BackgroundServices/TokenCleanupService.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/BackgroundServices/TokenCleanupService.cs) | Background token cleanup |

**AppDbContext** — EF Core ka main class:
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Identity
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Organization
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();

    // Employee
    public DbSet<EmployeeProfile> Employees => Set<EmployeeProfile>();

    // Attendance
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    // Leave
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();

    // Payroll
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();

    // Performance
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

**Configurations (Fluent API)** — Kyon Data Annotations nahi?
```
Data Annotations approach:
[Required] [MaxLength(100)] → Domain model pollute hota hai

Fluent API approach:
builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
→ Domain clean rahta hai, configuration alag file mein
```

**All Repositories:**
| Repository | File Path |
|------------|-----------|
| GenericRepository | [`EMS.Infrastructure/Repositories/GenericRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/GenericRepository.cs) |
| EmployeeRepository | [`EMS.Infrastructure/Repositories/EmployeeRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/EmployeeRepository.cs) |
| AttendanceRepository | [`EMS.Infrastructure/Repositories/AttendanceRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/AttendanceRepository.cs) |
| LeaveRepository | [`EMS.Infrastructure/Repositories/LeaveRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/LeaveRepository.cs) |
| PayrollRepository | [`EMS.Infrastructure/Repositories/PayrollRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/PayrollRepository.cs) |
| PerformanceRepository | [`EMS.Infrastructure/Repositories/PerformanceRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/PerformanceRepository.cs) |
| DepartmentRepository | [`EMS.Infrastructure/Repositories/DepartmentRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/DepartmentRepository.cs) |
| DesignationRepository | [`EMS.Infrastructure/Repositories/DesignationRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/DesignationRepository.cs) |
| AuthRepository | [`EMS.Infrastructure/Repositories/AuthRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/AuthRepository.cs) |
| RefreshTokenRepository | [`EMS.Infrastructure/Repositories/RefreshTokenRepository.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.Infrastructure/Repositories/RefreshTokenRepository.cs) |

---

### Layer 4: EMS.API

**Kya hai:** HTTP requests receive karo, validate karo, service call karo, response do.

**📁 Source Files:**
| File | Purpose |
|------|---------|
| [`EMS.API/Program.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.API/Program.cs) | App entry point, middleware setup |
| [`EMS.API/Middleware/ExceptionMiddleware.cs`](file:///d:/CodeSpace/Parmanent-Field/C-Sharp/Dot_net/Employes-Management-System/New%20folder/EmployeeManagementSystemBackend/src/EMS.API/Middleware/ExceptionMiddleware.cs) | Global error handler |

**Program.cs Key Sections:**
```csharp
// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Infrastructure (DB + JWT + All Services + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Database Migration + Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SuperAdminSeeder.SeedAsync(db, builder.Configuration);
}
```

**ExceptionMiddleware** — Global error handler:
```
Kya hota hai bina iske:
- Unhandled exception → ugly stack trace → 500 error
- Frontend ko kuch samajh nahi aata

Kya hota hai iske saath:
- Koi bhi exception catch hoti hai
- Structured JSON response milta hai
- Log mein error record hoti hai
- User-friendly message milta hai
```

**API Versioning (v1):**
```
/api/v1/employees
/api/v1/departments

Kyon versioning?
- Kal v2 aayi aur breaking changes hue?
- Old clients still v1 use karte rahenge
- Backward compatibility maintain hoti hai
```

---

## 7. Database Design

### Entity Relationship Diagram (Text Format)

```
AppUser
  └── EmployeeId (FK, optional) → EmployeeProfile

Department
  └── [has many] EmployeeProfile (Restrict delete — employees hain toh delete nahi)

Designation
  └── DepartmentId (FK, optional) → Department

EmployeeProfile
  ├── DepartmentId (FK, required) → Department
  ├── DesignationId (FK, optional) → Designation
  ├── ReportingManagerId (FK, optional, self-reference) → EmployeeProfile
  └── UserId (FK, optional) → AppUser

AttendanceRecord
  └── EmployeeId (FK) → EmployeeProfile
  └── UNIQUE (EmployeeId, Date) ← Ek din mein ek record

LeaveType ← Master data
LeaveApplication
  ├── EmployeeId (FK) → EmployeeProfile
  ├── LeaveTypeId (FK) → LeaveType
  └── ApprovedById (FK, optional) → EmployeeProfile

SalaryStructure
  └── EmployeeId (FK) → EmployeeProfile

PayrollRecord
  ├── EmployeeId (FK) → EmployeeProfile
  └── UNIQUE (EmployeeId, Month, Year) ← Month mein ek record

Goal
  ├── EmployeeId (FK) → EmployeeProfile
  └── SetByManagerId (FK, optional) → EmployeeProfile

PerformanceReview
  ├── EmployeeId (FK) → EmployeeProfile
  ├── ReviewerId (FK) → EmployeeProfile (Restrict delete)
  └── UNIQUE (EmployeeId, ReviewCycle) ← Ek cycle mein ek review
```

### DB Tables (12 total)

| Table | Records Kaise Bante Hain |
|---|---|
| Users | Register API se |
| Departments | HR Admin banata hai |
| Designations | HR Admin banata hai |
| Employees | HR Admin banata hai |
| AttendanceRecords | Clock In/Out se ya manual |
| LeaveTypes | HR Admin banata hai (Casual, Sick, etc.) |
| LeaveApplications | Employee apply karta hai |
| SalaryStructures | HR Admin set karta hai |
| PayrollRecords | Monthly payroll run se |
| Goals | Manager set karta hai |
| PerformanceReviews | Manager conduct karta hai |
| `__EFMigrationsHistory` | EF Core auto-manage karta hai |

### Migrations History

| Migration Name | Kya Hua |
|---|---|
| `InitialCreate` | Users, Departments, Designations, Employees, Attendance, Leave, Salary, Goals tables |
| `OrganizationModule` | Department + Designation relations fix |
| `EmployeeModule` | Self-reference FK (ReportingManager) |
| `AttendanceModule` | Employee FK + Cascade delete |
| `LeaveModule` | Dual Employee FK (applicant + approver) |
| `PayrollModule` | PayrollRecords table + unique constraint |
| `PerformanceModule` | Goal updates + PerformanceReviews table |
| `Enterprise_Identity_Upgrade` | Identity enhancements and refresh token infrastructure |
| `Fix_RefreshToken_Configuration` | Refresh token table configuration fixes |
| `FixDateOnlyTypes` | Date type handling fixes for PostgreSQL compatibility |

---

## 8. API Modules

### Module 1: Identity (Auth + Users)

**Kya karta hai:**
- User registration with BCrypt password hashing
- JWT token generation with claims
- Login with credential verification
- User management (list, deactivate)

**Key Business Logic:**
```
Register flow:
1. Email duplicate check
2. Password BCrypt hash
3. Role assignment (default: Employee)
4. JWT token generate
5. Return token + user info

Login flow:
1. Email se user fetch
2. IsActive check (inactive user login nahi kar sakta)
3. BCrypt.Verify (plain password vs hash)
4. LastLogin update
5. JWT token return
```

---

### Module 2: Organization (Departments + Designations)

**Kya karta hai:**
- Department CRUD with employee count, pagination + search
- Department delete guard (prevents deletion if employees assigned)
- Soft delete, restore, and purge for departments (data recovery)
- Designation CRUD with department association, pagination + search
- Soft delete, restore, and purge for designations (data recovery)
- Deleted items view (superadmin only)
- Unique name constraints per department

**Key Business Logic:**
```
Department delete guard:
- Agar department mein employees hain → delete block
- Error: "Cannot delete department with active employees"
→ Accidental data loss prevent
```

---

### Module 3: Employee Lifecycle

**Kya karta hai:**
- Employee full CRUD
- Auto employee code generation (EMP001, EMP002...)
- Multi-filter search (name, email, dept, status, gender)
- Self-reference reporting manager
- Soft delete (IsDeleted + Status = Inactive)

**Key Business Logic:**
```
Auto Employee Code:
totalCount = GetTotalCountAsync() (IgnoreQueryFilters — deleted bhi count hote hain)
code = $"EMP{(totalCount + 1):D3}"

Kyon IgnoreQueryFilters?
- Agar EMP001 delete ho aur dobara employee aaye?
- Bina IgnoreQueryFilters: firse EMP001 generate hoga → DUPLICATE!
- IgnoreQueryFilters se: deleted bhi count → next unique code milta hai
```

---

### Module 4: Attendance

**Kya karta hai:**
- Clock In / Clock Out
- Auto working hours calculate
- HalfDay detection (< 4 hours = HalfDay)
- Monthly summary with stats (admin)
- Self-service: get my today's record and monthly summary
- Admin manual attendance marking
- Auto-absent marking (background job runs daily at 11 PM IST)

**Key Business Logic:**
```
Working Hours Calculation:
clockOut - clockIn = total duration (TimeSpan)
workingHours = diff.TotalHours (rounded to 2 decimal)

Status Logic:
workingHours >= 4 → Present
workingHours < 4  → HalfDay

Duplicate check:
HasClockedInTodayAsync → prevent double clock-in
HasClockedOutTodayAsync → prevent double clock-out
```

---

### Module 5: Leave Management

**Kya karta hai:**
- Leave apply with balance check
- Overlap detection
- Manager approve/reject flow
- Employee cancel (with restrictions)
- Leave balance per type per year
- Leave type master management

**Key Business Logic:**
```
Balance Check:
used = approved leaves in current year
pending = pending leaves in current year
remaining = maxDaysPerYear - used - pending
if (totalDays > remaining) → error

Overlap Check:
Koi aur leave hai in dates ke beech?
→ Rejected/Cancelled leaves except kiye jaate hain
→ Active/Pending leaves overlap check hota hai

Cancel Restrictions:
- Sirf apni leave cancel kar sakte hain
- Approved leave jo already start ho gayi → cancel nahi
- Rejected leave → cancel nahi
```

---

### Module 6: Payroll

**Kya karta hai:**
- Salary structure set karna (Basic + Allowances)
- Monthly payroll run (single employee ya sab)
- Auto PF deduction (12% of Basic)
- Auto Tax deduction (10% of Adjusted Gross)
- LOP (Loss of Pay) calculation
- Payslip per employee per month
- Mark as Paid

**Key Business Logic:**
```
Payroll Calculation Formula:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
GrossSalary = Basic + HRA + Transport + Medical + Other

totalWorkingDays = weekdays in month (Sat/Sun exclude)
perDaySalary = GrossSalary / totalWorkingDays
lopDeduction = perDaySalary × lopDays

adjustedGross = GrossSalary - lopDeduction

pfDeduction = BasicSalary × 12%
taxDeduction = adjustedGross × 10%
totalDeductions = pfDeduction + taxDeduction + lopDeduction

netSalary = adjustedGross - (pfDeduction + taxDeduction)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Note: Simplified tax slab — real mein income tax slabs complex hoti hain
```

---

### Module 7: Performance

**Kya karta hai:**
- Goal set karna (manager employee ke liye)
- Progress update
- Performance review (5 categories, 1-5 scale)
- Overall rating auto-calculate
- Self comment by employee
- Submit review
- Employee performance summary

**Key Business Logic:**
```
Overall Rating = Average of 5 ratings:
(Technical + Communication + Teamwork + Leadership + Punctuality) / 5

Rating Labels:
>= 4.5 → "Outstanding"
>= 3.5 → "Exceeds Expectations"
>= 2.5 → "Meets Expectations"
>= 1.5 → "Needs Improvement"
< 1.5  → "Unsatisfactory"

Review Cycle Format: "2026-Q1", "2026-Q2", etc.
Unique constraint: (EmployeeId + ReviewCycle) → ek cycle mein ek review
```

---

### Module 8: Dashboard

**Kya karta hai:**
- Real-time stats (employee count, attendance today, pending leaves)
- Department headcount
- Recent activities (new joinees, leave applications, payroll)

**Kya Stats Milte Hain:**
```
Employee Stats:
- Total employees
- Active employees
- New joinees this month
- On probation

Today's Attendance:
- Present today
- Absent today
- On leave today
- Attendance % today

Leave Stats:
- Pending requests
- Approved this month

Payroll Stats:
- Total payroll this month (₹)
- Processed count

Performance:
- Pending reviews (Draft status)
- Goals completed this quarter
```

---

### Module 9: Reports

**Kya karta hai:**
- Attendance report (employee-wise, month-wise)
- Payroll report (department-wise)
- Headcount report (gender breakdown, status breakdown)

**Export-Ready Data:**
```
Reports ka data structured format mein aata hai
Frontend pe table mein show kar sakte hain ya CSV/Excel export kar sakte hain
(Future: direct PDF/Excel export)
```

---

## 9. Security Architecture

### JWT Token Structure

```json
{
  "sub": "1",
  "email": "admin@ems.com",
  "unique_name": "System Admin",
  "role": "SuperAdmin",
  "exp": 1744000000,
  "iss": "EMS.API",
  "aud": "EMS.Client"
}
```

### Role-Based Access Control

```
SuperAdmin → Sab kuch access
  └── User deactivation
  └── Department delete
  └── Designation delete
  └── Employee delete
  └── Payroll mark-paid

HRAdmin → HR operations
  └── Department create/update
  └── Employee create/update
  └── Payroll run + salary structure
  └── Manual attendance
  └── Leave types

Manager → Team management
  └── Leave approve/reject
  └── Goals set
  └── Performance reviews

Employee → Self-service
  └── Clock In/Out
  └── Leave apply + cancel
  └── View own payslips
  └── Self review comment
  └── Update goal progress
```

### Password Security

```
BCrypt work factor: 10 (default)
Cost: ~100ms per hash (brute force difficult)
Salt: auto-generated per password
Plain text: NEVER stored

Attack scenarios:
- Rainbow table: Protected (unique salt per password)
- Brute force: Protected (BCrypt deliberately slow)
- SQL injection: Protected (EF Core parameterized queries)
```

### Token Validation

```
Backend validates:
✅ Issuer (EMS.API)
✅ Audience (EMS.Client)
✅ Expiry time
✅ Signing key (HMACSHA256)
✅ Token format
```

---

## 10. Request Flow — Ek Request Ka Poora Safar

### Example: Employee Create

```
Client (Frontend/Postman)
    │
    │ POST /api/v1/employees
    │ Authorization: Bearer eyJhbGci...
    │ Body: { firstName, lastName, email, ... }
    │
    ▼
ExceptionMiddleware.InvokeAsync()
    │ (try block — koi exception aaye toh catch karega)
    │
    ▼
CORS Middleware
    │ (Origin check — allowed hai?)
    │
    ▼
Authentication Middleware
    │ JWT token validate karo
    │ Claims extract karo (userId, role, email)
    │
    ▼
Authorization Middleware
    │ [Authorize(Roles = "SuperAdmin,HRAdmin")] check
    │ Role "Employee" hai → 403 Forbidden return
    │
    ▼
EmployeesController.Create()
    │ ModelState.IsValid check
    │ (Required fields, Email format, etc.)
    │
    ▼
IEmployeeService.CreateAsync(dto)
    │ (Application Layer — Business Logic)
    │
    ├── Email duplicate check karo
    │   ├── Duplicate hai → return (null, "Email already registered.")
    │   └── Unique hai → continue
    │
    ├── GetTotalCountAsync() → auto code generate
    │   └── "EMP006"
    │
    ├── EmployeeProfile object create karo
    │   (dto → domain entity)
    │
    ▼
IEmployeeRepository.CreateAsync(employee)
    │ (Infrastructure Layer — DB)
    │
    ├── _context.Employees.AddAsync(employee)
    ├── _context.SaveChangesAsync()
    └── GetByIdAsync(id) → reload with navigations
        (Department, Designation, ReportingManager)
    │
    ▼
MapToDto(employeeProfile)
    │ EmployeeProfile → EmployeeResponseDto
    │ (PasswordHash? Domain mein nahi, DTO mein nahi)
    │
    ▼
Controller returns:
    CreatedAtAction(201 Created)
    │
    ▼
ApiResponse<EmployeeResponseDto>.Ok(result, "Employee created.")
    │
    ▼
JSON Response to Client:
{
  "success": true,
  "message": "Employee created successfully.",
  "data": {
    "id": 6,
    "employeeCode": "EMP006",
    "fullName": "Jane Smith",
    ...
  },
  "errors": []
}
```

---

## 11. Design Patterns Used

### 1. Repository Pattern

```
Kya hai: Data access layer abstraction
Kyon: Controller/Service ko direct DB access nahi
Faida: Test mein mock repo use kar sakte hain

Interface:
IEmployeeRepository → Contract define karta hai

Implementation:
EmployeeRepository → Actual EF Core queries

Controller jaanta hai sirf IEmployeeRepository
Real DB ya Mock DB — farq nahi padta
```

### 2. Unit of Work Pattern

```
Kya hai: Multiple operations ek transaction mein
Kyon: Data consistency

Example:
- Payroll run karo 100 employees ke liye
- 50th employee mein error aaye?
- Bina UoW: 50 saved, 50 nahi → inconsistent state
- UoW ke saath: Sab rollback ya sab commit
```

### 3. Dependency Injection

```
Kya hai: Objects inject karo, khud mat banao
Kyon: Loose coupling, testability

// Ye mat karo (tight coupling):
var service = new EmployeeService(new EmployeeRepository(new AppDbContext(...)));

// Yeh karo (DI):
public EmployeesController(IEmployeeService service)
{
    _service = service; // Framework inject karega
}
```

### 4. DTO Pattern (Data Transfer Object)

```
Domain Model:    Employee (PasswordHash, IsDeleted, etc. bhi hai)
Response DTO:    EmployeeResponseDto (sirf safe fields)

Kyon zarooori?
- Sensitive data (PasswordHash) frontend tak nahi jaata
- API response ka shape fix hota hai
- Domain changes se API break nahi hoti
```

### 5. Factory Method (ApiResponse)

```csharp
// Consistent response banane ka factory
ApiResponse<T>.Ok(data, message)   // Success response
ApiResponse<T>.Fail(message)       // Error response

Kyon?
- Har controller mein manually response object banana tedious tha
- Consistent format ensure hota hai
```

### 6. Strategy Pattern (LeaveStatus)

```
Leave status workflow:
Pending → Approved/Rejected (Manager action)
Pending → Cancelled (Employee action)
Approved → Cancelled (Employee, if not started)
Rejected → Final (kuch nahi ho sakta)
```

---

## 12. Performance & Scalability

### Current Optimizations

#### 1. Pagination — Har List Endpoint Pe

```
GET /api/v1/employees?page=1&pageSize=10

Kyon zarooori?
1000 employees hain → sab ek saath fetch karo?
- DB pe load
- Network pe bandwidth
- Frontend render slow

Pagination se:
- Sirf 10 records fetch
- Fast response
- Smooth UX
```

#### 2. Selective Include (Eager Loading)

```csharp
// BAD — N+1 problem
var employees = await _context.Employees.ToListAsync();
foreach (var e in employees)
{
    var dept = await _context.Departments.FindAsync(e.DepartmentId); // N queries!
}

// GOOD — Single query with JOIN
var employees = await _context.Employees
    .Include(e => e.Department)
    .Include(e => e.Designation)
    .ToListAsync();
```

#### 3. AsQueryable() — Deferred Execution

```csharp
var query = _context.Employees.AsQueryable();

// Conditions add karte jao — DB pe nahi jaata abhi
if (filter.DepartmentId.HasValue)
    query = query.Where(e => e.DepartmentId == filter.DepartmentId);

// Yahan DB pe jaata hai — ek single optimized query
var result = await query.ToListAsync();
```

#### 4. Database Indexes

```
Unique Indexes (lookup fast + duplicates prevent):
- Users.Email
- Employees.Email
- Employees.EmployeeCode
- Departments.Name
- AttendanceRecords.(EmployeeId, Date)
- PayrollRecords.(EmployeeId, Month, Year)
- PerformanceReviews.(EmployeeId, ReviewCycle)

FK Indexes (JOIN queries fast):
- Employees.DepartmentId
- Employees.DesignationId
- Employees.ReportingManagerId
- LeaveApplications.EmployeeId
- LeaveApplications.ApprovedById
- etc.
```

#### 5. Soft Delete + Global Query Filter

```csharp
// EF Core configuration
builder.HasQueryFilter(e => !e.IsDeleted);

// Har query mein automatically add hota hai:
WHERE "IsDeleted" = FALSE

// Developer ko manually add nahi karna
// Galti se deleted records fetch nahi hote
```

#### 6. Async/Await Throughout

```csharp
// Sab DB operations async hain
await _context.Employees.ToListAsync()
await _context.SaveChangesAsync()

// Kyon?
- Thread block nahi hota wait mein
- Same thread dusri request handle kar sakta hai
- High concurrency possible
```

### Scalability Analysis

```
Current Setup (Single Server):
├── RAM: 512MB minimum
├── CPU: 1-2 cores
├── Concurrent Users: ~500-1000 (realistic)
└── DB Connections: 20-50 (default EF Core pool)

With Horizontal Scaling (Multiple Servers + Load Balancer):
├── JWT Stateless → Multiple servers kaam karte hain
├── DB: Neon PostgreSQL → managed scaling
└── Concurrent Users: ~5,000-10,000

Production Recommendations (v2):
├── Redis Cache → Frequently accessed data cache karo
│   (Department list, LeaveTypes, etc.)
├── Connection Pooling → PgBouncer ya Neon's built-in
├── Rate Limiting → API abuse prevent
└── CDN → Static assets
```

### Response Time Benchmarks (Expected)

| Operation | Expected Response Time |
|---|---|
| GET /health/ping | < 10ms |
| GET /employees (paginated) | < 100ms |
| POST /auth/login | 150-300ms (BCrypt) |
| POST /auth/register | 200-400ms (BCrypt hash) |
| POST /payroll/run (100 employees) | 2-5 seconds |
| GET /dashboard/stats | 100-200ms |

---

## 13. API Endpoints Reference

### Base URL: `http://localhost:5185/api/v1`

### 🔓 Public (No Auth Required)

| Method | Endpoint | Description |
|---|---|---|
| POST | `/auth/register` | New user register |
| POST | `/auth/login` | Login, get JWT |
| GET | `/health/ping` | Server health check |
| GET | `/health/db` | Database connection check |

### 🔐 Identity Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/users` | SuperAdmin, HRAdmin | All users |
| GET | `/users/me` | All Auth | Current user profile |
| GET | `/users/{id}` | SuperAdmin, HRAdmin | User by ID |
| PATCH | `/users/{id}/deactivate` | SuperAdmin | Deactivate user |
| PATCH | `/users/{id}/role` | SuperAdmin | Change user role |

### 🏢 Organization Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/departments` | All Auth | Get all (paginated + search) |
| GET | `/departments/{id}` | All Auth | Get by ID |
| POST | `/departments` | SuperAdmin, HRAdmin | Create |
| PUT | `/departments/{id}` | SuperAdmin, HRAdmin | Update |
| DELETE | `/departments/{id}` | SuperAdmin | Soft delete (if no employees) |
| GET | `/departments/deleted` | SuperAdmin | Get deleted departments |
| POST | `/departments/{id}/restore` | SuperAdmin | Restore deleted department |
| DELETE | `/departments/purge` | SuperAdmin | Purge old deleted departments |
| GET | `/designations` | All Auth | Get all (filter by dept) |
| POST | `/designations` | SuperAdmin, HRAdmin | Create |
| PUT | `/designations/{id}` | SuperAdmin, HRAdmin | Update |
| DELETE | `/designations/{id}` | SuperAdmin | Soft delete |
| GET | `/designations/deleted` | SuperAdmin, HRAdmin | Get deleted designations |
| POST | `/designations/{id}/restore` | SuperAdmin | Restore deleted designation |
| DELETE | `/designations/purge` | SuperAdmin | Purge old deleted designations |

### 👤 Employee Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/employees` | All Auth | Get all (paginated + multi-filter) |
| GET | `/employees/{id}` | All Auth | Get by ID |
| POST | `/employees` | SuperAdmin, HRAdmin | Create + auto code |
| PUT | `/employees/{id}` | SuperAdmin, HRAdmin | Update |
| DELETE | `/employees/{id}` | SuperAdmin, HRAdmin | Soft delete |

### 🕐 Attendance Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| POST | `/attendance/clock-in` | All Auth | Clock in |
| POST | `/attendance/clock-out` | All Auth | Clock out + hours calc |
| GET | `/attendance/my/today` | All Auth | Get my today's record |
| GET | `/attendance/my/summary` | All Auth | Get my monthly summary |
| GET | `/attendance` | Admin, Manager | All records (filtered) |
| GET | `/attendance/today/{empId}` | All Auth | Today's record |
| GET | `/attendance/summary/{empId}` | All Auth | Monthly summary |
| POST | `/attendance/manual` | SuperAdmin, HRAdmin | Manual mark |

### 🏖️ Leave Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/leave` | All Auth | All applications |
| GET | `/leave/{id}` | All Auth | By ID |
| POST | `/leave/apply` | All Auth | Apply leave |
| PATCH | `/leave/{id}/action` | Admin, Manager | Approve/Reject |
| PATCH | `/leave/{id}/cancel` | All Auth | Cancel own leave |
| GET | `/leave/balance/{empId}` | All Auth | Leave balance |
| GET | `/leave/types` | All Auth | Leave types |
| POST | `/leave/types` | SuperAdmin, HRAdmin | Create leave type |

### 💰 Payroll Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| POST | `/payroll/salary-structure` | SuperAdmin, HRAdmin | Set salary |
| GET | `/payroll/salary-structure` | SuperAdmin, HRAdmin | Get structures |
| POST | `/payroll/run` | SuperAdmin, HRAdmin | Run monthly payroll |
| GET | `/payroll` | SuperAdmin, HRAdmin | All records |
| GET | `/payroll/{id}` | All Auth | By ID |
| GET | `/payroll/payslip/{empId}` | All Auth | Monthly payslip |
| GET | `/payroll/payslips/{empId}` | All Auth | All payslips |
| PATCH | `/payroll/{id}/mark-paid` | SuperAdmin, HRAdmin | Mark as paid |

### 📈 Performance Module

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/performance/goals` | All Auth | Goals list |
| POST | `/performance/goals` | Admin, Manager | Set goal |
| PATCH | `/performance/goals/{id}/progress` | All Auth | Update progress |
| DELETE | `/performance/goals/{id}` | Admin, Manager | Delete goal |
| GET | `/performance/reviews` | All Auth | Reviews list |
| POST | `/performance/reviews` | Admin, Manager | Create review |
| PATCH | `/performance/reviews/{id}/self-comment` | All Auth | Add self comment |
| PATCH | `/performance/reviews/{id}/submit` | Admin, Manager | Submit review |
| GET | `/performance/summary/{empId}` | All Auth | Performance summary |

### 📊 Dashboard & Reports

| Method | Endpoint | Roles | Description |
|---|---|---|---|
| GET | `/dashboard/stats` | Admin, Manager | Overall stats |
| GET | `/dashboard/headcount` | Admin, Manager | Dept headcount |
| GET | `/dashboard/activities` | Admin, Manager | Recent activities |
| GET | `/reports/attendance` | SuperAdmin, HRAdmin | Attendance report |
| GET | `/reports/payroll` | SuperAdmin, HRAdmin | Payroll report |
| GET | `/reports/headcount` | SuperAdmin, HRAdmin | Headcount report |

---

## 14. Capacity & Load Analysis

### Single Server Estimate

```
Hardware: 2 vCPU, 2GB RAM, SSD

Concurrent Active Users: ~500
Requests per second: ~200-300
DB queries per second: ~500-1000 (multiple queries per request)
Uptime: 99.5% (single server, no redundancy)
```

### What Can This Handle?

```
Small Company (< 100 employees):
✅ Perfect — overkill bhi hai

Medium Company (100-500 employees):
✅ Perfect — ample capacity

Large Company (500-2000 employees):
⚠️ Need optimization:
   - Add Redis cache for dashboard
   - Connection pooling tune karo
   - DB indexes review karo

Enterprise (2000+ employees):
❌ Current architecture insufficient
   - Need microservices
   - Need message queue (payroll processing)
   - Need read replicas for reports
```

### Memory Usage

```
.NET 10 runtime: ~100MB baseline
Application code: ~50MB
DB connection pool: ~20MB
Per request overhead: ~1-5KB
Total estimate: ~200-300MB RAM comfortable
```

---

## 15. Known Limitations & Future Improvements

### Current Implementation Status

```
✅ IMPLEMENTED & WORKING:
- Refresh token mechanism — Silent token refresh with queue system
- Background token cleanup service — Expired tokens auto-deleted
- Auto-absent marking service — Automatically marks absent for no-shows daily at 11 PM IST
- Global exception middleware — All errors centralized
- Soft delete with query filters — Designations + Departments
- Dashboard service with real-time stats
- Report generation service (Attendance, Payroll, Headcount)
- Auto SuperAdmin seeding on startup
- Employee code auto-generation (EMP001, EMP002, etc.)
- Leave overlap detection
- Duplicate attendance prevention (EmployeeId + Date unique)
- Payroll record uniqueness (EmployeeId + Month + Year)
- Performance review cycle uniqueness

🔴 CRITICAL LIMITATIONS:
- No rate limiting — API abuse possible
- No input sanitization middleware (XSS prevention)
- CORS too open in development mode (hardcoded to localhost:8080 only)

🟡 IMPORTANT LIMITATIONS:
- Tax calculation simplified (flat 10%) — not real Indian tax slabs
- No email notifications (leave approval, payroll, etc.)
- No file upload (employee photo, documents)
- No audit log — who changed what when
- Working days calculation doesn't include holidays
- No multi-company support
- Integration tests directory empty (xUnit setup ready, but no test cases)
- Performance module unit tests missing

🟢 NICE TO HAVE:
- Export to Excel/PDF for reports
- Bulk employee import (CSV)
- Employee org chart API
- Shift management
- Overtime calculation
- Mobile push notifications
```

### Planned Improvements (Future Versions)

#### v1.1 (Short Term)
- [x] ✅ Refresh token implementation — COMPLETED
- [ ] Rate limiting (per user, per endpoint)
- [ ] Email service integration (SMTP/SendGrid)
- [ ] Holiday calendar management
- [ ] Employee document upload
- [ ] Input sanitization middleware (XSS prevention)

#### v1.2 (Medium Term)
- [ ] Real tax calculation with Indian tax slabs (progressive rates)
- [ ] Audit log for all changes (who, what, when, why)
- [ ] Bulk operations (bulk employee import/update)
- [ ] Excel/PDF export for reports
- [ ] Complete integration tests suite
- [ ] Performance review module unit tests
- [ ] API documentation with Swagger UI (in addition to Scalar)

#### v2.0 (Long Term)
- [ ] Multi-tenant support (multiple companies)
- [ ] Microservices architecture (separate services per domain)
- [ ] Event-driven payroll processing (message queue - RabbitMQ/Azure Service Bus)
- [ ] Employee org chart visualization API
- [ ] Mobile API optimizations (data compression, caching)
- [ ] Real-time notifications (SignalR/WebSocket)
- [ ] Holiday calendar with working days automation
- [ ] Shift management and shift-based attendance
- [ ] Overtime calculation and compensation

---

## 16. Changelog — Version History

---

### v1.0.0 — Initial Release
**Date:** March 2026
**Status:** ✅ Complete

#### 🏗️ Architecture Setup (Day 1-2)
- Clean Architecture solution (4 projects: Domain, Application, Infrastructure, API)
- EF Core 10 + PostgreSQL setup
- Generic Repository + UnitOfWork pattern
- Global Exception Middleware
- JWT Authentication setup with refresh token mechanism
- Silent token refresh with request queuing for concurrent requests
- Background service for auto-cleanup of expired refresh tokens
- BCrypt password hashing with workFactor: 12
- Auto SuperAdmin seeding on startup (superadmin@ems.com)

#### 👤 Identity Module (Day 3)
- User registration with role assignment
- JWT login with claims
- User listing with role filter
- User deactivation

#### 🏢 Organization Module (Day 4)
- Department CRUD with pagination + search + sort
- Department delete guard (prevents deletion if employees assigned)
- Soft delete implementation for departments
- Designation CRUD with department filter
- Soft delete + Restore for designations (data recovery feature)
- Purge old deleted designations (>12 months)
- Unique name constraints per department
- Query filters to exclude soft-deleted records

#### 👥 Employee Lifecycle Module (Day 5)
- Employee CRUD with auto EmployeeCode (EMP001...)
- Multi-filter search (name, email, dept, status, gender)
- Self-referencing reporting manager
- Employee soft delete

#### 🕐 Attendance Module (Day 6)
- Clock In / Clock Out
- Auto working hours calculation
- HalfDay detection (< 4 hours)
- Monthly attendance summary
- Admin manual attendance marking
- Unique constraint (EmployeeId + Date)

#### 🏖️ Leave Management Module (Day 7)
- Leave application with balance validation
- Overlap detection (prevents duplicate leaves)
- Manager approve/reject workflow
- Employee cancel with business rules
- Per-type per-year balance tracking
- Leave type master management

#### 💰 Payroll Module (Day 8)
- Salary structure management
- Monthly payroll run (batch processing)
- PF deduction (12% Basic), Tax (10% Adjusted Gross)
- LOP (Loss of Pay) calculation
- Per-day salary for partial months
- Payslip per employee per month
- Mark as Paid workflow

#### 📈 Performance Module (Day 9)
- Goal setting with progress tracking
- 5-category performance review (1-5 scale)
- Auto overall rating calculation
- Rating labels (Outstanding → Unsatisfactory)
- Employee self-comment
- Review submit workflow
- Employee performance summary

#### 📊 Dashboard + Reports (Day 10)
- Real-time dashboard stats (employee count, attendance, on-leave)
- Department headcount breakdown with API
- Recent activity feed with timestamps
- Attendance report (employee-wise with daily breakdown)
- Payroll report (department-wise salary analysis)
- Headcount report (gender + employment status breakdown)
- Aggregate analytics service with pagination support

#### 🧪 Unit Tests
- xUnit + Moq + FluentAssertions setup with Coverlet code coverage
- AuthService tests (registration, login, failed attempts, lockout - 7 cases)
- DepartmentService tests (CRUD, delete guards, soft delete - 7 cases)
- EmployeeService tests (CRUD, auto code generation, filters - 5 cases)
- LeaveService tests (balance validation, overlap detection, workflow - 7 cases)
- TestDataBuilder helper for consistent test data
- Test coverage: ~26 test cases covering core business logic

---

### Implementation Notes

#### Refresh Token Architecture (v1.0.0)
The refresh token mechanism implemented provides enterprise-grade token management:

**Features:**
- **Silent Refresh**: When access token expires, automatically rotates without user re-login
- **Request Queuing**: During token refresh, concurrent requests are queued and re-executed with new token
- **Dual Token System**: Access token (15 min) + Refresh token (7 days) stored separately
- **Background Cleanup**: TokenCleanupService removes expired tokens every hour
- **Automatic Logout**: On refresh token expiration or failure, user is auto-logged out

**Security Benefits:**
- Short-lived access tokens reduce exposure window
- Refresh token stored securely in database (not in JWT)
- Automatic cleanup prevents token table bloat
- Failed refresh triggers immediate logout (compromised token = logout)

**Frontend Integration:**
- Axios interceptor catches 401, queues requests, refreshes silently
- User experiences smooth UX — no interruption on token refresh
- Fallback: After multiple failures, redirect to login

---

Jab bhi koi change karo, neeche format mein add karo:

```markdown
### v1.x.x — Short Description
**Date:** DD Month YYYY
**Type:** 🐛 Bug Fix / ✨ Feature / ♻️ Refactor / 🔒 Security

#### Changes
- Description of what changed
- Why it was changed
- What problem it solves

#### Breaking Changes (if any)
- List any API breaking changes

#### Migration Steps (if needed)
- Steps to update existing installations
```

---

## 📞 Quick Reference

### Environment Setup

```bash
# Backend run karo
cd EmployeeManagementSystemBackend
dotnet run --project src/EMS.API

# API Docs
http://localhost:5185/scalar/v1

# New migration
dotnet ef migrations add MigrationName --project src/EMS.Infrastructure --startup-project src/EMS.API

# DB update
dotnet ef database update --project src/EMS.Infrastructure --startup-project src/EMS.API

# Tests run karo
dotnet test tests/EMS.UnitTests/EMS.UnitTests.csproj --verbosity normal

# Build
dotnet build
```

### appsettings.json Key Settings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=EMS_Dev;Username=...;Password=..."
  },
  "JwtSettings": {
    "Secret": "minimum-32-character-secret-key!!",
    "Issuer": "EMS.API",
    "Audience": "EMS.Client",
    "ExpiryInMinutes": 60
  }
}
```

### First Time Setup Sequence

```
1. Register SuperAdmin
   POST /api/v1/auth/register
   { "role": "SuperAdmin" }

2. Login
   POST /api/v1/auth/login

3. Departments create karo

4. Designations create karo

5. Leave Types create karo
   (Casual Leave, Sick Leave, Earned Leave)

6. Employees add karo

7. Salary structures set karo

8. Start tracking attendance & leave!
```

---

*Last Updated: April 2026 | Version: 1.0.0*
*Document Owner: Backend Development Team*

> **Note:** Yeh document ek living document hai. Har feature, bug fix, ya architectural decision ke saath isse update karo. Future self aur team members ko bahut help milegi.

---

## 17. Code Reference Index

### Complete File Tree

```
EmployeeManagementSystemBackend/src/
├── EMS.API/                              # Presentation Layer
│   ├── Controllers/v1/
│   │   ├── AuthController.cs             # POST register, login, refresh, logout
│   │   ├── UsersController.cs            # GET users, PATCH role, deactivate
│   │   ├── DepartmentsController.cs      # CRUD departments
│   │   ├── DesignationsController.cs     # CRUD designations + restore/purge
│   │   ├── EmployeesController.cs        # CRUD employees + filters
│   │   ├── AttendanceController.cs       # Clock in/out, manual, summary
│   │   ├── LeaveController.cs           # Apply, approve/reject, cancel, balance
│   │   ├── PayrollController.cs          # Salary structure, run, payslip
│   │   ├── PerformanceController.cs      # Goals, reviews, summary
│   │   ├── DashboardController.cs        # Stats, headcount, activities
│   │   ├── ReportsController.cs          # Attendance, payroll, headcount reports
│   │   └── HealthController.cs           # Ping, db connection check
│   ├── Middleware/
│   │   └── ExceptionMiddleware.cs        # Global error handler
│   ├── Program.cs                        # App entry point
│   └── appsettings.json
│
├── EMS.Application/                      # Application Layer
│   ├── Common/
│   │   ├── DTOs/
│   │   │   ├── ApiResponse.cs            # Standard response wrapper
│   │   │   └── PaginatedResult.cs        # Pagination wrapper
│   │   └── Interfaces/
│   │       └── IJwtService.cs            # JWT service interface
│   └── Modules/
│       ├── Identity/
│       │   ├── DTOs/                     # Register, Login, Refresh, UserResponse
│       │   ├── Interfaces/               # IAuthService, IUserService, IRefreshTokenRepository
│       │   └── Services/                 # AuthService, UserService
│       ├── Organization/
│       │   ├── DTOs/                     # Department, Designation DTOs
│       │   ├── Interfaces/               # IDepartmentService, IDesignationService
│       │   └── Services/                 # DepartmentService, DesignationService
│       ├── Employees/
│       │   ├── DTOs/                     # Create, Update, Filter, Response DTOs
│       │   ├── Interfaces/               # IEmployeeService, IEmployeeRepository
│       │   └── Services/                 # EmployeeService
│       ├── Attendance/
│       │   ├── DTOs/                     # ClockIn, ClockOut, Filter, Summary DTOs
│       │   ├── Interfaces/               # IAttendanceService, IAttendanceRepository
│       │   └── Services/                 # AttendanceService
│       ├── Leave/
│       │   ├── DTOs/                     # Apply, Action, Balance, Filter DTOs
│       │   ├── Interfaces/               # ILeaveService, ILeaveRepository
│       │   └── Services/                 # LeaveService
│       ├── Payroll/
│       │   ├── DTOs/                     # SalaryStructure, RunPayroll, Record DTOs
│       │   ├── Interfaces/               # IPayrollService, IPayrollRepository
│       │   └── Services/                 # PayrollService
│       ├── Performance/
│       │   ├── DTOs/                     # Goal, Review, Summary DTOs
│       │   ├── Interfaces/               # IPerformanceService, IPerformanceRepository
│       │   └── Services/                 # PerformanceService
│       ├── Dashboard/
│       │   ├── DTOs/                     # Stats, Headcount, Activity DTOs
│       │   └── Interfaces/               # IDashboardService
│       └── Reports/
│           ├── DTOs/                     # Attendance, Payroll, Headcount Report DTOs
│           └── Interfaces/               # IReportService
│
├── EMS.Domain/                          # Domain Layer
│   ├── Common/
│   │   └── BaseEntity.cs                # Id, CreatedAt, UpdatedAt, IsDeleted
│   ├── Entities/
│   │   ├── Identity/
│   │   │   ├── AppUser.cs               # User entity
│   │   │   └── RefreshToken.cs          # Refresh token entity
│   │   ├── Organization/
│   │   │   ├── Department.cs            # Department entity
│   │   │   └── Designation.cs           # Designation entity
│   │   ├── Employee/
│   │   │   └── EmployeeProfile.cs        # Employee profile entity
│   │   ├── Attendance/
│   │   │   └── AttendanceRecord.cs      # Attendance record entity
│   │   ├── Leave/
│   │   │   ├── LeaveApplication.cs      # Leave application entity
│   │   │   └── LeaveType.cs             # Leave type entity
│   │   ├── Payroll/
│   │   │   ├── PayrollRecord.cs         # Payroll record entity
│   │   │   └── SalaryStructure.cs       # Salary structure entity
│   │   └── Performance/
│   │       ├── Goal.cs                  # Goal entity
│   │       └── PerformanceReview.cs     # Performance review entity
│   └── Enums/
│       ├── UserRole.cs                  # SuperAdmin, HRAdmin, Manager, Employee
│       ├── EmploymentStatus.cs          # Active, OnProbation, Resigned, Terminated
│       ├── Gender.cs                     # Male, Female, Other
│       ├── LeaveStatus.cs               # Pending, Approved, Rejected, Cancelled
│       └── AttendanceStatus.cs          # Present, Absent, HalfDay, Holiday, OnLeave
│
└── EMS.Infrastructure/                   # Infrastructure Layer
    ├── DependencyInjection.cs           # All DI registrations
    ├── Persistence/
    │   ├── AppDbContext.cs              # EF Core DbContext
    │   └── Configurations/              # Fluent API configurations
    ├── Repositories/
    │   ├── GenericRepository.cs          # Base repository
    │   ├── AuthRepository.cs            # Auth data access
    │   ├── RefreshTokenRepository.cs    # Refresh token operations
    │   ├── EmployeeRepository.cs        # Employee data access
    │   ├── AttendanceRepository.cs      # Attendance data access
    │   ├── LeaveRepository.cs           # Leave data access
    │   ├── PayrollRepository.cs         # Payroll data access
    │   ├── PerformanceRepository.cs     # Performance data access
    │   ├── DepartmentRepository.cs      # Department data access
    │   └── DesignationRepository.cs     # Designation data access
    ├── Services/
    │   ├── JwtService.cs                # JWT token generation/validation
    │   ├── Dashboard/
    │   │   └── DashboardService.cs      # Dashboard analytics
    │   └── Reports/
    │       └── ReportService.cs         # Report generation
    ├── BackgroundServices/
    │   ├── AutoAbsentMarkingService.cs  # Auto-mark absent for no-shows daily
    │   └── TokenCleanupService.cs       # Background token cleanup
    ├── Seeders/
    │   └── SuperAdminSeeder.cs          # SuperAdmin seeding
    └── UnitOfWork/
        └── UnitOfWork.cs                # Transaction management
```

### Quick Code Lookup

| What You Need | File Location |
|---------------|---------------|
| API Entry Point | `EMS.API/Program.cs` |
| All Endpoints | `EMS.API/Controllers/v1/*.cs` |
| Error Handling | `EMS.API/Middleware/ExceptionMiddleware.cs` |
| Database Setup | `EMS.Infrastructure/Persistence/AppDbContext.cs` |
| DI Configuration | `EMS.Infrastructure/DependencyInjection.cs` |
| JWT Logic | `EMS.Infrastructure/Services/JwtService.cs` |
| All Entities | `EMS.Domain/Entities/**/*.cs` |
| All Enums | `EMS.Domain/Enums/*.cs` |
| Business Logic | `EMS.Application/Modules/*/Services/*.cs` |
| Data Access | `EMS.Infrastructure/Repositories/*.cs` |
| Response Format | `EMS.Application/Common/DTOs/ApiResponse.cs` |
| Auto Seeding | `EMS.Infrastructure/Seeders/SuperAdminSeeder.cs` |
| Background Jobs | `EMS.Infrastructure/BackgroundServices/*.cs` |

### All DTOs Reference

| Module | DTOs |
|--------|------|
| **Identity** | RegisterDto, LoginDto, LogoutDto, RefreshTokenRequestDto, AuthResponseDto, UserResponseDto, ChangeRoleDto |
| **Organization** | CreateDepartmentDto, UpdateDepartmentDto, DepartmentResponseDto, CreateDesignationDto, DesignationResponseDto |
| **Employee** | CreateEmployeeDto, UpdateEmployeeDto, EmployeeFilterDto, EmployeeResponseDto |
| **Attendance** | ClockInDto, ClockOutDto, ManualAttendanceDto, AttendanceFilterDto, AttendanceResponseDto, MonthlyAttendanceSummaryDto |
| **Leave** | ApplyLeaveDto, LeaveActionDto, LeaveFilterDto, LeaveResponseDto, LeaveBalanceDto, LeaveTypeResponseDto, CreateLeaveTypeDto |
| **Payroll** | CreateSalaryStructureDto, RunPayrollDto, PayrollFilterDto, PayrollRecordResponseDto, SalaryStructureResponseDto |
| **Performance** | CreateGoalDto, UpdateGoalProgressDto, GoalResponseDto, CreateReviewDto, SelfCommentDto, ReviewResponseDto, PerformanceFilterDto, EmployeePerformanceSummaryDto |
| **Dashboard** | DashboardStatsDto, DepartmentHeadcountDto, RecentActivityDto |
| **Reports** | AttendanceReportDto, PayrollReportDto, HeadcountReportDto |

### All Interfaces Reference

| Module | Service Interface | Repository Interface |
|--------|-------------------|---------------------|
| **Identity** | IAuthService, IUserService | IAuthRepository, IRefreshTokenRepository |
| **Organization** | IDepartmentService, IDesignationService | IDepartmentRepository, IDesignationRepository |
| **Employee** | IEmployeeService | IEmployeeRepository |
| **Attendance** | IAttendanceService | IAttendanceRepository |
| **Leave** | ILeaveService | ILeaveRepository |
| **Payroll** | IPayrollService | IPayrollRepository |
| **Performance** | IPerformanceService | IPerformanceRepository |
| **Dashboard** | IDashboardService | - |
| **Reports** | IReportService | - |
| **Core** | IJwtService | - |

---

*End of Documentation*
