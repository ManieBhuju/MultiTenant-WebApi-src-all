MultiTenant Web API
====================

Overview
--------
Multi-tenant ASP.NET Core Web API uses a "database-per-tenant" approach. The master database (MasterDb) stores Identity (ApplicationUser) and tenant metadata (Tenants). 
Each tenant has its own Postgres database that contains only the Employees table and is addressed by a connection string saved in the master Tenants.DbConnStr.

Prerequisites
-------------
- .NET 10 SDK
- PostgreSQL server accessible from the app (a maintenance DB connection string with privileges to CREATE/DROP/ALTER databases is required for tenant lifecycle operations)
- A configured appsettings.json or environment variables for database connection strings

Key environment variables / appsettings (examples)
- ConnectionStrings:MasterConnection -> Postgres connection string for the master DB (used by MasterDbContext)
- ConnectionStrings:PostgresMaintenance -> Postgres connection string used by TenantService to CREATE/ALTER/DROP tenant DBs
- Jwt:Secret, Jwt:Issuer, Jwt:Audience -> JWT configuration used by authentication

Setup and run
-------------
1. Update appsettings.json or set environment variables with your Postgres credentials and JWT secret.
2. From solution root, build the solution:
   dotnet build
3. Run the API project (from src/Apps/MultiTenant.Api)
   dotnet run --project src/Apps/MultiTenant.Api

When the application starts it will:
- Run EF Core migrations for the master database (MasterDbContext)
- Attempt to migrate each tenant DB found in MasterDb.Tenants
- Seed initial identity roles and users using IdentitySeeder

Initial SuperAdmin credentials
------------------------------
During startup IdentitySeeder creates base roles and a SuperAdmin account when not present. Default seeded credentials :
- Email: assessment@yopmail.com
- Password: Tester@123

Authentication
--------------
The API uses JWT bearer authentication. Use the /api/account/login endpoint to authenticate and receive a token. 
Include the token in requests using the Authorization: Bearer {token} header (Paste only token).

Example API requests (curl)
---------------------------
1) Login (get token)

curl -X POST https://localhost:5001/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"assessment@yopmail.com","password":"Tester@123"}'

Response contains JWT token.

2) Create a tenant (SuperAdmin only)

curl -X POST https://localhost:5001/api/tenants \
  -H "Authorization: Bearer {superadmin_token}" \
  -H "Content-Type: application/json" \
  -d '{"tenantId":"SuperAdmin","displayName":"SuperAdmin","adminEmail":"assessment@yopmail.com","adminPassword":"assessment@yopmail.com"}'

Successful response includes tenant metadata. The tenant's database will be created and the provided admin user will be created in the master DB and assigned to the tenant.

3) Login as tenant admin and get employees list

- Login as tenant admin using /api/account/login => obtain token (token will include tenant claim)
- Get employees (Employee role required)

curl -X GET https://localhost:5001/api/employee \
  -H "Authorization: Bearer {tenant_admin_token}"

This returns employees from the tenant's own database (the tenant DB is resolved using the tenant claim and MasterDb.Tenants.DbConnStr).

4) Create employee (Admin role)

curl -X POST https://localhost:5001/api/employee/register \
  -H "Authorization: Bearer {tenant_admin_token}" \
  -H "Content-Type: application/json" \
  -d '{"emailAddress":"jane@acme.local","fullName":"Jane Doe","role":"Employee"}'

Architecture overview
---------------------
- Master database (MasterDbContext)
  - Stores AspNetIdentity tables (ApplicationUser) and Tenants table (tenant metadata, DbConnStr)
  - ApplicationUser has TenantId FK (points to Tenants.Id) so users are associated to a tenant record in the master DB

- Tenant databases (one per tenant)
  - Each tenant database contains only the Employees table (migrated/created using TenantDbContext)
  - Connection string for a tenant DB is stored in Tenants.DbConnStr in the master DB

- Tenant resolution
  - The application resolves the current tenant using a claim (tenant id) present in the authenticated user's JWT or HttpContext (TenantProvider/TenantResolver). The resolved tenant record is used to configure TenantDbContext per-request.

- Services
  - TenantService: creates/renames/drops tenant DBs using a maintenance Postgres connection and configures the tenant's DbConnStr in the master DB. It calls TenantDbContext.Database.EnsureCreated/ Migrate for the tenant DB to ensure the Employees table exists.
  - Application handlers: Employee CRUD handlers use ITenantDbContext (registered per-request) so all employee operations target the tenant DB.

Tenant database creation flow
----------------------------
When creating a tenant the flow is:
1. SuperAdmin calls CreateTenant API endpoint with tenantId and admin user details.
2. TenantService uses a maintenance Postgres connection to CREATE DATABASE "tenant_{tenantId}" or similar.
3. TenantService builds a connection string for the new tenant DB and runs EF Core EnsureCreatedAsync or MigrateAsync using TenantDbContext configured to include only Employee model configuration. This creates the Employees table only in the tenant DB.
4. A Tenants record is saved in the master DB with the generated DbConnStr.
5. Tenant admin user is created in the master Identity tables and its ApplicationUser.TenantId is set to the master Tenants.Id (so Identity FK points to the master tenant record).

Important notes and caveats
--------------------------
- Tenant lifecycle operations (CREATE/DROP/ALTER) require appropriate Postgres privileges. The code uses pg_terminate_backend when dropping a DB but you still need sufficient privileges.
- Cross-database operations (modifying both master Identity and tenant DB) are not covered by a single distributed transaction in many environments. The project uses compensation or TransactionScope in some handlers, but on some hosts distributed transactions (MSDTC) may not be available. Test and adapt accordingly.
- Validation: the project uses FluentValidation and a MediatR validation pipeline behavior. Add validators for your commands/DTOs to enable request validation.
- Logging and monitoring: ExceptionMiddleware logs exceptions to a file; you may want to replace or augment with structured logging (Serilog) and centralized sinks.

Troubleshooting
---------------
- If tenant creation fails check the maintenance connection string and Postgres user privileges.
- If you see FK violations while creating users, ensure Tenants record is created and persisted before setting ApplicationUser.TenantId.

Contributing
------------
- Follow repository coding style (C# with existing conventions).
- Add unit and integration tests for tenant lifecycle and cross-db flows.

License
-------
Project code in this workspace is under the repository's license. Review the repository root for a LICENSE file.
