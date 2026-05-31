# CLAUDE.md

Guidance for Claude Code in this repository. This file documents the shared
local database setup; see the repo README for full project documentation.

## Local database & services (shared dev-services stack)

**Do NOT spin up new Postgres/Redis containers for local dev.** All repos in
`~/Repos` share one stack defined in `~/Repos/dev-services` (one PostGIS + Redis +
MinIO + Adminer on the `devnet` Docker network). Start it once with `make up`
there.

This microservice solution (Tasky) uses one Postgres server with a database per
service (`localhost:5432`, `postgres`/`postgres`):

- **`TaskyAdministrationDb`**
- **`TaskyIdentityServiceDb`**
- **`TaskyProjectsDb`**
- **`TaskySaaSDb`**

All four are pre-created by the shared stack's init SQL (mixed-case names quoted
so they match exactly). Redis is also provided by the shared stack at
`localhost:6379`.

### ⚠️ Aspire caveat

Local dev normally runs via `dotnet run` in `src/apps/Tasky.AppHost/`, which uses
**.NET Aspire** to provision its OWN Postgres, Redis, and RabbitMQ containers and
inject `ConnectionStrings__*` env vars — it does **not** use the shared stack, and
its Postgres collides on 5432 if the shared stack is up. To consolidate, configure
the AppHost / services to use the existing connection strings above instead of
`AddPostgres(...)` provisioning. (The per-service `docker-compose.yml` files under
`src/services/*` start MS SQL Server and are legacy template artifacts, not the
local-dev path.)

If a port is taken by an old per-project container, stop that container rather
than remapping ports. To add a database, edit `dev-services/init/01-databases.sql`.
