# Replicate SP Endpoint Pattern

Use this as a checklist whenever you add a new endpoint that calls a SQL Server stored procedure. The existing **User Info** endpoint (`UsersController`, `UserService`, `UserResponse`, `UserInfoSpResult`) is the reference implementation.

---

## Pattern Overview

```
Client → Controller → Service → DatabaseContext (SqlQueryRaw + SqlParameter) → SQL Server
       ←            ←          ← flat DTO → map to response                  ←
```

- **Controller**: Thin; calls service, returns 200/404/500, logs exceptions.
- **Service**: Builds params from request/claims, executes SP via `SqlQueryRaw<T>`, maps flat DTO → response.
- **Database**: Same connection (from `Database` config or env vars); no new config per endpoint.

---

## Checklist for Each New Endpoint

### 1. Response model (public API)

- Add a **record** in `Models/Responses/` (or a new file there).
- Reuse `OrganizationUnit` for Id+Name shapes; add new records only when the shape is different.
- Use nullable types for optional SP columns.

### 2. Flat SP result DTO (internal mapping)

- Add a **class** (settable properties, parameterless constructor) in `Models/Data/`.
- Property names must match the SP result set column names exactly.
- Use `string?` for nullable columns.

### 3. Service interface and implementation

- Add a method to the appropriate **interface** in `Services/Interfaces/` (or create a new interface if it's a new area, e.g. `ILocationService`).
- Signature pattern: `Task<ResponseType?> GetXxxAsync(ClaimsPrincipal? user)` (or with other params as needed).
- In the **service** (existing or new under `Services/`):
  - Resolve input (e.g. email from `user`, or route/query params passed from controller).
  - Create one `SqlParameter` per SP parameter: `new SqlParameter("@ParamName", SqlDbType.XXX, size) { Value = value }`.
  - Call: `database.Database.SqlQueryRaw<FlatDto>("EXEC [dbo].[SP_Name] @Param1, @Param2", param1, param2).FirstOrDefaultAsync()` (or `.ToListAsync()` for multiple rows).
  - Map flat DTO → response in a private `MapToXxx(FlatDto row)` method.
  - Wrap in try/catch: log with `ILogger`, then rethrow.

### 4. Controller

- Add an action that:
  - Calls the service (passing `User` if the SP needs the current user).
  - Returns `Ok(result)` when result is not null, `NotFound()` when null.
  - Catches exceptions, logs with the controller `ILogger`, returns `StatusCode(500, "An error occurred while ...")`.
- Use **primary constructor** for the controller; keep a single responsibility per controller (e.g. Users, Locations, Reports).
- Apply `[Authorize(Policy = "EDExternalPolicy")]` at controller level (or override per action if needed).

### 5. DI registration

- In `Program.cs` **#region Services**, register the new interface and implementation:
  `builder.Services.AddScoped<IYourService, YourService>();`

---

## Conventions to Follow

| Concern | Rule |
|--------|------|
| **SQL injection** | Never concatenate or interpolate user/request data into the SQL string. Always pass values via `SqlParameter`. |
| **Auth / user context** | When the SP needs "current user," pass `ClaimsPrincipal user` from the controller and read claims (e.g. `ClaimTypes.Email`) in the service. Use a fallback only for local/dev. |
| **Database connection** | Use existing `DatabaseContext`; connection string comes from `Database:Host/Database/User/Password` in appsettings or env vars `Database__Host`, etc. |
| **Errors** | Service: log and rethrow. Controller: catch, log, return 500 with a generic message. |
| **Naming** | SP result DTO: `XxxSpResult`. Response: `XxxResponse`. Map method: `MapToXxx`. |

---

## Example: Adding a "List locations" endpoint

Illustrative steps (adjust names to your real SP and API):

1. **Response**: e.g. `record LocationResponse(string Id, string Name)` or reuse `OrganizationUnit`.
2. **Flat DTO**: e.g. `class LocationSpResult { public string LocationID { get; set; } public string LocationName { get; set; } }` in `Models/Data/`.
3. **Interface**: e.g. `Task<IReadOnlyList<LocationResponse>> GetLocationsAsync();`
4. **Service**: Execute `EXEC [dbo].[USP_HH_Locations_Get]` with no params (or with `SqlParameter` if the SP has parameters). Map each row to `LocationResponse`; return list.
5. **Controller**: e.g. `GET api/Locations` → call service, return `Ok(result)` or handle errors as above.
6. **Program.cs**: Register the new service in **#region Services**.

No new appsettings or env vars are required for the database; they are already defined for all SP-based endpoints.

---

## File layout summary

| Layer | Location | Purpose |
|-------|----------|---------|
| Response DTOs | `Models/Responses/` | Public API shape; use records. |
| SP result DTOs | `Models/Data/` | Flat classes matching SP result set. |
| Interfaces | `Services/Interfaces/` | Contract for the service. |
| Services | `Services/` | SP execution, mapping, logging. |
| Controllers | `Controllers/` | Route, auth, call service, 200/404/500. |
| DI | `Program.cs` #region Services | Register `IXxxService`, `XxxService`. |

Following this plan keeps new endpoints consistent with the existing User Info endpoint and the refactoring/code-smells rules.
