# Multi-Tenant Migration Guide

This document explains how to manage Entity Framework Core migrations in our database-per-tenant architecture.

## 1. Creating a Migration

When you change the Domain Entities or the EF Core Configurations in `HRMS.Infrastructure`, you need to create a new migration.

Open your terminal, navigate to the solution root, and run:

```bash
dotnet ef migrations add <MigrationName> --project src/HRMS.Infrastructure --startup-project src/HRMS.API
```

*(Note: The API project needs to be referenceable as the startup project to provide the necessary configuration, but the migrations will be saved in the `HRMS.Infrastructure` project).*

## 2. Applying Migrations

Because we use a database-per-tenant strategy, you **cannot** simply run `dotnet ef database update`. Doing so would only attempt to update a single database (and would fail if no default connection string is provided).

Instead, migrations are applied programmatically to **all** tenant databases using the `MultiTenantMigrator` service.

### On Application Startup (Development/Staging)

In the `Program.cs` of the `HRMS.API` project (to be implemented in Phase 4), you can resolve the migrator and apply migrations automatically:

```csharp
using var scope = app.Services.CreateScope();
var migrator = scope.ServiceProvider.GetRequiredService<MultiTenantMigrator>();
await migrator.MigrateAllAsync();
```

### On-Demand via API (Production)

For production, it is recommended to expose an authenticated, admin-only endpoint that triggers the `MultiTenantMigrator.MigrateAllAsync()` method, allowing you to control exactly when the schema update rolls out across all tenant databases.
