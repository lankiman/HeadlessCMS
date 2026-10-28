# HeadlessCMS

A multi-tenant, schema-driven headless CMS API built with .NET 10, PostgreSQL, MongoDB and GraphQL.
Redis holds short-lived auth state.

## Projects

Dependencies flow one way: `API → Services → Data → Common`.

| Project | Job |
|---|---|
| `HeadlessCMS.API` | HTTP surface: endpoints, request/response contracts and their validators, mapping, startup wiring |
| `HeadlessCMS.Services` | The application's logic: auth, workspaces, API tokens |
| `HeadlessCMS.Data` | EF Core context, entities, entity configurations, value objects, Redis and PostgreSQL setup |
| `HeadlessCMS.Common` | Things every layer shares: enums, typed settings |
| `HeadlessCMS.Tests` | Unit and integration tests |

Empty folders hold a `.gitkeep` file so Git keeps them. Delete it when you add the first real file.

## Running it

The app needs PostgreSQL and Redis. Settings come from `appsettings.json`, environment variables, or a `.env` file (which is git-ignored):

```
ConnectionStrings__DefaultConnection=Host=localhost;Database=headlesscms;Username=...;Password=...
ConnectionStrings__Redis=localhost:6379
```

```
dotnet run --project HeadlessCMS.API
```

## Logging

The API uses [Logship](https://github.com/lankiman/logship-csharp) to write log files and ship logs to Grafana Loki.
Both are off until configured:

| Setting | Meaning |
|---|---|
| `Logship__File__Enabled` | `true` writes JSON log lines to files |
| `Logship__File__Directory` | folder for the files (default `logs`) |
| `Logship__Loki__Url` | Loki address; leave empty to keep Loki off |
| `Logship__Loki__AppLabel`, `Logship__Loki__JobLabel` | labels on the logs (default `headless-cms`) |
| `Logship__Loki__AuthType` | `None`, `Basic` or `Token` |
| `Logship__Loki__Username`, `Password`, `Token`, `OrgId` | credentials; set these as secrets, not in the repo |

In Grafana, query a service's logs with `{app_name="headless-cms"}`.

## Conventions

- **Soft delete.** Call `context.SoftDelete(entity)`. It sets `DeletedAt` and keeps the row, and every query hides such rows.
  Add `.IgnoreQueryFilters()` to a query to see them. A plain `Remove()` throws on save, so a row is never deleted by accident.
  `context.HardDelete(entity)` really deletes a row. A new entity needs `builder.HasSoftDeleteFilter()` in its configuration (a test fails if it is missing).
  Unique indexes ignore deleted rows, so a deleted user's email can be registered again. `ExecuteDelete` and raw SQL bypass these rules.
- **Timestamps.** `CreatedAt` and `UpdatedAt` are set automatically on save.
- **Enums** are stored as strings.
