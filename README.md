# 🏗️ UCMS — Unified Construction Management System

Construction management REST API for projects, 
brigades, work logs, estimates, payments, and organizations.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL
- ASP.NET Core Identity + JWT Authentication

## Architecture

```
src/
├── Ucms.Domain/          # Entities, enums, domain interfaces
├── Ucms.Application/     # Business logic, services, abstractions
├── Ucms.Infrastructure/  # Database, EF Core, migrations, seed data
└── Ucms.Api/             # Controllers, authentication, Swagger
```

## Getting Started

**Requirements:** .NET 8 SDK, PostgreSQL 14+

**Database migration** — in Visual Studio's Package Manager 
Console (Default project: `Ucms.Infrastructure`, Startup project: `Ucms.Api`):

```powershell
Add-Migration InitialMigration
Update-Database
```

**Seed data** — enable in `appsettings.json`; 
It runs automatically at startup:

```json
{ "SeedData": { "Enabled": true } }
```

## Authentication

JWT-based. Open Swagger at `https://localhost:{PORT}/swagger`, 
click **Authorize**, and log in.

## Test Accounts

> ⚠️ Seed credentials for local development only. 
Disable seeding (`SeedData.Enabled = false`) and rotate these before any deployment.

| Username   | Password       | Role       | Scope                            |
|------------|----------------|------------|----------------------------------|
| sysadmin   | SysAdmin123!   | Admin      | All organizations (System Owner) |
| admin      | Admin123!      | Admin      | Organization management          |
| manager    | Manager123!    | Manager    | Projects, estimates, brigades    |
| brigadir   | Brigadir123!   | Brigadir   | Work logs                        |
| accountant | Accountant123! | Accountant | Acts and payments                |

## Main API Endpoints

| Method | Endpoint            | Description   |
|--------|---------------------|---------------|
| POST   | `/api/auth/login`   | Login         |
| POST   | `/api/auth/refresh` | Refresh token |
| GET    | `/api/projects`     | Projects      |
| GET    | `/api/brigades`     | Brigades      |
| GET    | `/api/worklogs`     | Work logs     |
| GET    | `/api/payments`     | Payments      |
| GET    | `/api/dashboard`    | Statistics    |

## Coding Style

### XML Comments

Use bilingual XML comments (Uzbek + Russian):

```csharp
/// <summary>
/// Ball yig'ish.
/// Начислить баллы.
/// </summary>
```

### Methods

Use block-bodied methods.

```csharp
// Preferred
public void Execute()
{
}

// Avoid
public void Execute() => DoSomething();
```

### Accessibility Modifiers

Always specify explicit accessibility modifiers.

```csharp
// Preferred
public class UserService
{
}

// Avoid
class UserService
{
}
```

### Constructors

Use primary constructors (C# 12 / .NET 8). 
Avoid explicit constructors unless additional initialization logic is required.

```csharp
// Preferred
public class Service(ILogger<Service> logger)
{
}

// Avoid
public class Service
{
    private readonly ILogger<Service> _logger;

    public Service(ILogger<Service> logger)
    {
        _logger = logger;
    }
}
```

## Author

**Xabibullayev Davronbek** — davronbekxabibullayev03.06.88@gmail.com
