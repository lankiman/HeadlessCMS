# Guide for AI coding agents

Read this file and `docs/PLAN.md` before you change anything.

## What this project is

HeadlessCMS is a multi-tenant, schema-driven headless CMS API: .NET 10, PostgreSQL (users, auth, schemas, platform
config), MongoDB (user-defined record data), Redis (short-lived auth state), HotChocolate GraphQL.
It has no dashboard. It is a data layer: the apps that use it own their business logic.
The tenant is a **workspace**, not a user. Every user gets a personal workspace on registration.

## Layers

`HeadlessCMS.API → HeadlessCMS.Services → HeadlessCMS.Data → HeadlessCMS.Common`. Dependencies go one way only.
`ArchitectureTests` fails the build if a layer uses one above it, or if the API uses Data directly.

| Project | Holds |
|---|---|
| API | endpoints, `Contracts/<Feature>/` (request and response DTOs and their validators), `Mapping/`, startup wiring |
| Services | application logic and the interfaces the API calls |
| Data | EF Core context, entities, `Configurations/`, value objects, Redis and MongoDB setup |
| Common | enums, typed settings |

## Conventions (follow them exactly)

- **DTOs** are `record`s. **Entities** are classes deriving from `BaseEntity`. **Value objects** such as `Email` are records.
- **Mapping is manual**, with extension methods in `API/Mapping/`. Never use AutoMapper or any mapping library.
  Never map a secret (password hash, token hash, 2FA secret) into a response.
- **Entities never leave Services.** A service takes an input record and returns a result record (both defined in
  `Services/<Feature>/`). The API maps request → input and result → response. The API must not use an entity.
- A record that holds a secret (a password, a token) overrides `ToString()` so the secret is never printed.
- **Reference example:** `API/Contracts/Auth/` (request, validator, response), `API/Mapping/AuthMappings.cs`,
  `Services/Auth/` (input and result), and the tests in `Tests/Unit/Contracts/`. Copy this pattern for every new feature.
- **Validation** uses FluentValidation: one validator per request DTO, in the same folder. Validators check the shape of a
  request. Rules that need the database belong in the service, and unique indexes are the final safeguard.
- Each layer registers its own services in `Extensions/ServiceCollectionExtensions.cs`.
- **Nullable reference types** are on. Do not use `[Required]` for nullability.
- **Enums are stored as strings.** Dates are UTC.
- **Soft delete:** call `context.SoftDelete(entity)`, never `Remove()` (it throws). New entities call `builder.HasSoftDeleteFilter()`
  in their configuration. Unique indexes use `SoftDeleteExtensions.NotDeletedSql` as their filter.
- **Logging:** take `ILogger<T>`. Logship is configured only in the API project. Never log secrets, tokens or passwords.
- **Settings** come from environment variables or user-secrets. Never commit a secret.
- Audit events use the `EventType` enum and store their data as `jsonb`.

## Who writes what

The maintainer writes the security-critical logic by hand. If a task needs one of these, define the interface, call it,
and leave the body as `throw new NotImplementedException();` with a `// HUMAN:` comment. Do not implement them:

- generating and hashing API tokens, refresh tokens and recovery codes
- refresh-token rotation and revoke-all
- the registration transaction (user, personal workspace and owner membership together)
- resolving a caller's membership, and the workspace filter on queries
- the two-step 2FA login

You write everything around them: DTOs, validators, mapping, endpoints, OpenAPI metadata, EF configurations and migrations,
tests and documentation.

## Tests

- xunit. Unit tests live in `HeadlessCMS.Tests/Unit`, tests that run real EF Core against in-memory SQLite in `HeadlessCMS.Tests/Integration`.
- Cover the success path, each validation rule, and the failure paths (unauthenticated, forbidden, not found, conflict).
- Three tests are written by the maintainer, so do not write them: a token from workspace A cannot reach workspace B, a password
  change revokes every refresh token, and a `Read` API token cannot write.

## Commands

```
docker compose up -d          # PostgreSQL, Redis and MongoDB for local work
dotnet build
dotnet test
```

## Definition of done

`dotnet build` and `dotnet test` pass. Public types and members have XML comments. `docs/` and the README are updated when
behaviour or setup changes. You list the files you changed and anything you were unsure about. You change nothing outside the task.
