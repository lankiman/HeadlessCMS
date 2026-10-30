# HeadlessCMS

A multi-tenant, schema-driven headless CMS API built with .NET 10, PostgreSQL, MongoDB and GraphQL.
Redis holds short-lived auth state.

## Projects

Dependencies flow one way: `API → Services → Data → Common`.

| Project | Job |
|---|---|
| `HeadlessCMS.API` | HTTP surface: endpoints, request/response contracts and their validators, mapping, startup wiring |
| `HeadlessCMS.Services` | The application's logic: auth, workspaces, API tokens |
| `HeadlessCMS.Data` | EF Core context, entities, entity configurations, value objects, PostgreSQL, Redis and MongoDB setup |
| `HeadlessCMS.Common` | Things every layer shares: enums, typed settings |
| `HeadlessCMS.Tests` | Unit and integration tests |

Empty folders hold a `.gitkeep` file so Git keeps them. Delete it when you add the first real file.

## Running it

You need Docker for PostgreSQL, Redis and MongoDB.

```
cp .env.example .env            # then change the passwords
docker compose up -d
dotnet run --project HeadlessCMS.API
```

Settings come from `appsettings.json`, environment variables, or the git-ignored `.env` file at the repo root:

| Setting | Meaning |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ConnectionStrings__Redis` | Redis connection string (defaults to `localhost:6379`) |
| `ConnectionStrings__Mongo` | MongoDB connection string (defaults to localhost in Development, required elsewhere) |
| `Mongo__Database` | MongoDB database name (default `headlesscms`) |

On start-up the app pings Redis and MongoDB, applies PostgreSQL migrations, and logs the result.

## Documentation

- [`docs/PLAN.md`](docs/PLAN.md): decisions, status and the roadmap.
- [`AGENTS.md`](AGENTS.md) and [`docs/AGENT_PROMPTS.md`](docs/AGENT_PROMPTS.md): rules and prompts for AI coding agents.

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
