# HeadlessCMS plan

The single place for what is decided, what is done and what comes next.
Update it when a decision changes. `AGENTS.md` has the rules for AI agents, and `docs/AGENT_PROMPTS.md` has the prompts.

## The product

A schema-driven, multi-tenant headless CMS API. A user defines schemas (content types and their fields) and the platform
serves records for them through generated REST endpoints and GraphQL. There is no dashboard, and it is not meant to be
extended by editing its code. Apps that use it keep their own business logic, roles and workflows.

It follows the Grafana model: the same code can be self-hosted, or run as a hosted service. Billing is out of scope.
The project is a portfolio piece, so a clear README, passing tests and one deployed demo matter more than extra features.

## Settled decisions

**Stack and layout**
- .NET 10 Web API. Layers: API → Services → Data → Common, enforced by tests.
- PostgreSQL through EF Core: users, auth, schemas, platform config. MongoDB through the native driver: record data.
  Redis: short-lived auth state, and it is required. HotChocolate for GraphQL.
- The fixed API (auth, workspaces, tokens) uses controllers. The generated record routes use endpoint routing.
- Manual DTO mapping. FluentValidation, with validators beside their DTOs. Enums stored as strings. Nullable reference types.
- `Email` is an owned value object. Roles come from a seeded table. Audit events are typed, with `jsonb` data.
- Soft delete through `SoftDelete()`; unique indexes ignore deleted rows. `CreatedAt` and `UpdatedAt` are set on save.
- Logging through `ILogger<T>`, with Logship (file and Loki sinks) configured in the API only.

**Tenancy and auth**
- The tenant is a workspace. Registration creates the user, a personal workspace and an owner membership in one transaction.
- Two kinds of caller. People use a JWT, and the workspace comes from the route. Apps use an API token, which belongs to one workspace.
- API tokens: random, stored only as a hash in PostgreSQL, shown once, `Read` or `ReadWrite`, revocable, workspace-wide for now.
- Short-lived JWT access token plus a refresh token held in Redis. Auth is hand-rolled, with an established password hasher.
- 2FA with an authenticator app (TOTP). Recovery codes are stored hashed and are single-use.
- The workspace role is read from the membership table on every request, never from the JWT.
- No platform-level roles, no invitations and no teams yet.

## Status

**Done**
- Cleaned-up repository, folder structure, and layer rules with tests.
- DbContext with `User`, `Role` and `AuditLog`, their configurations, timestamps and soft delete (tested on SQLite).
- PostgreSQL, Redis and MongoDB wiring with start-up checks, one extension file per layer.
- Logship installed. `docker-compose.yml` and `.env.example` for local services.
- CI that builds and tests, and shows failing tests as annotations.

**Phase 0: foundation (left)**
1. Typed settings checked at start-up: `Jwt`, `Auth`, `Security`. The app refuses to start with a clear message.
2. Health endpoints: `/health` and `/health/ready` (PostgreSQL, MongoDB, Redis).
3. A baseline migration, and the policy for applying it (automatic in development, an explicit step in production).
4. Move `Microsoft.AspNetCore.OpenApi` to 10.x. This needs a small change in `AddApiServices`.
5. Decide whether to tighten `global.json`.
6. Document the Railway settings.

**Phase 1: auth**

Tables (PostgreSQL): `User` (exists), `RecoveryCode`, `Workspace`, `Membership`, `Role` (exists, seed `Owner`), `Permission`
(seeded from the `PermissionKey` enum), `RolePermission`, `ApiToken`.

Redis keys (prefix `cms:`): refresh token by its hash, plus a per-user set so all of a user's tokens can be revoked;
pending 2FA login with a short expiry; rate-limit counters.

| Route | Does |
|---|---|
| `POST /auth/register` | user, personal workspace and owner membership in one transaction |
| `POST /auth/login` | password check, then tokens or a pending-2FA step |
| `POST /auth/login/2fa` | authenticator code or recovery code, then tokens |
| `POST /auth/refresh` | rotate the refresh token |
| `POST /auth/logout` | revoke the refresh token |
| `GET /auth/me` | the user and their workspaces |
| `POST /auth/2fa/enroll`, `/confirm`, `/disable` | TOTP setup |
| `POST /auth/2fa/recovery-codes` | regenerate codes, shown once |
| `POST /auth/password/change` | change password, revoke all refresh tokens |
| `POST/GET/DELETE /workspaces/{id}/api-tokens` | create (shown once), list, revoke |

Rules: management routes accept only a JWT, record routes accept only an API token. Rotating a refresh token invalidates the old one,
and reuse of an old one revokes the family. A password change, a 2FA reset or disabling 2FA revokes all refresh tokens.
Login and the 2FA step are rate-limited. Errors never reveal whether an email exists. No secret appears in a log, an error or a `ToString`.
Audit events: login succeeded and failed, password change, 2FA changes, token created and revoked.

Build order: 1.1 entities and migration → 1.2 register → 1.3 login, JWT and refresh → 1.4 `me` and permission policies
→ 1.5 membership resolution → 1.6 2FA → 1.7 API tokens and the second scheme → 1.8 rate limits and audit → 1.9 docs.

**Phase 2: the core product**
- Schemas as metadata in PostgreSQL, scoped to a workspace. Records in MongoDB, scoped to a workspace and a schema.
- Generated REST endpoints at `/api/{schemaNamePlural}`, with OpenAPI documents generated per workspace.
- GraphQL for schema management and for record queries and mutations.
- Validation of records against their schema. Pagination, filtering and sorting.
- Open design question: how records are laid out in MongoDB (a collection per schema, or one shared collection keyed by workspace and schema).

**Phase 3: self-hosting and polish**
- README and install script for self-hosting. A deployed demo. Tests against real services (Testcontainers).

## Who writes what

- **You, by hand:** token, refresh-token and recovery-code generation and hashing; refresh rotation and revoke-all; the registration
  transaction; membership resolution and the workspace filter; the two-step 2FA login. Also three tests: a token from workspace A cannot
  reach workspace B, a password change revokes every refresh token, and a `Read` token cannot write.
- **The AI agent:** DTOs, validators, mapping, endpoints, OpenAPI metadata, EF configurations and migrations, the rest of the tests, documentation.

## Open decisions (proposed defaults)

| Question | Proposed |
|---|---|
| Password hasher | the `PasswordHasher` class from the Microsoft identity packages, used on its own (verify before committing) |
| TOTP library | use a maintained NuGet library instead of writing it |
| API key header | `X-Api-Key` |
| Lifetimes | access token 15 minutes, refresh token 30 days, pending 2FA 5 minutes |
| Login limits | to be set with the rate limiter |
| Record routes | API token only |
| May a `Read` token read schema definitions? | yes, read-only (not decided) |

## Parked

- A platform admin view of all workspaces and accounts. Two designs were considered: a standard admin contract that every project
  implements, or adapters that describe a project's existing API. A platform-level credential would also be needed.
- Invitations and teams, platform roles, per-schema token scopes, the `Page` entity, a CLI, billing.
