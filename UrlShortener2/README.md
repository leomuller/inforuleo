# URL Shortener Microservice

ASP.NET Core (.NET 10) Web API that shortens URLs, with idempotent creation
(same long URL always returns the same short code) and PostgreSQL persistence.

> Built with AI assistance (Claude, GitHub Copilot, ChatGPT).

## Project layout

```
UrlShortener2/              the API
  Controllers/              UrlsController (the real API contract), TestController (scratch/debug endpoints)
  Services/                 UrlShorteningService - the business logic, DI-injected
  Data/                     AppDbContext (EF Core, maps to a hand-written DB schema - no EF migrations)
  Models/                   UrlMapping entity, UrlMappingStatus enum
  Dtos/                     request/response shapes for the API contract
  Utils/                    Base62, UrlValidator, UrlNormalizer - pure static functions, not business services
tests/
  UrlShortener2.UnitTests/        xUnit, no external dependencies
  UrlShortener2.IntegrationTests/ xUnit + WebApplicationFactory + Testcontainers.PostgreSql
```

## Build, run, test

Prerequisites: .NET 10 SDK, a PostgreSQL instance, Docker (only needed for the
integration tests, which spin up their own throwaway Postgres container).

### Database setup

Run once against your Postgres server:

```sql
CREATE ROLE apiuser WITH LOGIN PASSWORD 'apipwd';
CREATE DATABASE inforu OWNER apiuser;
```

Then, connected to the `inforu` database:

```sql
CREATE TABLE lt_status (
    status_id   SMALLINT PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL
);

INSERT INTO lt_status (status_id, status_name) VALUES
    (2, 'initializing'),
    (3, 'active'),
    (4, 'archived');
-- 1 is intentionally skipped, so it's never mistaken for a boolean.

CREATE TABLE url_mappings (
    mapping_id           BIGSERIAL PRIMARY KEY,
    url_short            VARCHAR(50)  NULL,
    url_long             TEXT         NOT NULL,
    url_long_normalized  TEXT         NOT NULL,
    date_created         TIMESTAMPTZ  NOT NULL,
    date_last_used       TIMESTAMPTZ  NULL,
    status_id            SMALLINT     NOT NULL REFERENCES lt_status (status_id)
);

CREATE UNIQUE INDEX ux_url_mappings_url_long_normalized ON url_mappings (url_long_normalized);
CREATE UNIQUE INDEX ux_url_mappings_url_short ON url_mappings (url_short) WHERE url_short IS NOT NULL;
```

Set the connection string in `UrlShortener2/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "UrlMapperDB": "Host=localhost;Port=5432;Database=inforu;Username=apiuser;Password=apipwd"
}
```

(`appsettings.json` only holds a `<placeholder>` sample - real credentials belong
in the environment-specific file / environment variables, never committed as-is
in a real deployment.)

### Run

```bash
dotnet run --project UrlShortener2
```

In Development, an OpenAPI UI is available via Scalar.AspNetCore at
`https://localhost:7006/scalar/#tag/urls`.

### Test

```bash
dotnet test
```

Runs both projects: unit tests (instant, no dependencies) and integration tests
(needs Docker running - Testcontainers starts and tears down a real Postgres
container automatically, so no manual DB setup is needed just to run them).

## API

### `POST /api/v1/urls` - shorten a URL

```bash
curl -i -X POST https://localhost:7006/api/v1/urls \
  -H "Content-Type: application/json" \
  -d '{"longUrl":"https://www.example.com/products/category/shoes?color=red&size=42"}'
```

First time: `201 Created`, `Location` header set, body:
```json
{
  "shortCode": "00000C",
  "shortUrl": "https://sho.rt/00000C",
  "longUrl": "https://www.example.com/products/category/shoes?color=red&size=42",
  "createdAt": "2026-10-23T09:15:00Z"
}
```

Same `longUrl` submitted again (any casing/whitespace variant that normalizes
the same): `200 OK`, same `shortCode`/`shortUrl`.

Invalid input: `400 Bad Request`, `ProblemDetails` body.

### `GET /{shortCode}` - resolve

```bash
curl -i https://localhost:7006/00000C
```

`302 Found` with `Location: <longUrl>` if active, `404 Not Found`
(`ProblemDetails`) if unknown, malformed, or not yet/no longer active.

### `GET /health`

```bash
curl -i https://localhost:7006/health
```

`200 Healthy` / `503 Unhealthy` - actually checks DB connectivity
(`AppDbContext.Database.CanConnectAsync()`), not just "is the process up".

## Short-code strategy

Each row gets a DB-generated `BIGSERIAL` (`mapping_id`). The short code is that
number Base62-encoded (`0-9A-Za-z`), left-padded with `'0'` to at least 6
characters. This means:

- Codes are unique by construction - no random generation, no collision
  retries needed, since the underlying ID is already unique.
- The code is reversible: `GET /{shortCode}` decodes it back to `mapping_id`
  and looks the row up by primary key, rather than by a text-matching scan.

## Idempotency and concurrency

"Same long URL -> same short URL" is enforced at two levels, not just in code:

1. **Normalization** (`UrlNormalizer`) - before comparing, the URL is reduced
   to a canonical form: lowercase scheme/host, trailing slash stripped *only*
   when the path is exactly `/` (root). `https://Example.com/` and
   `https://example.com` normalize identically; `https://example.com/foo/`
   and `https://example.com/foo` do **not** - only the root path's slash is
   special-cased, per the assignment's wording.
2. **A unique index on `url_long_normalized`** - this is what actually
   guarantees idempotency, not application code. The flow:
   - Check if a row with this normalized URL already exists -> return it.
   - If not, insert a new row with status `initializing` and no short code
     yet (the code depends on `mapping_id`, which only exists after insert).
   - Compute the short code from the newly-assigned `mapping_id`, set status
     to `active`, save again.
   - **Concurrent requests for the same URL**: both can pass the "does it
     exist" check before either inserts. Only one `INSERT` wins; the DB
     rejects the second with a unique-constraint violation. That failure is
     caught (`DbUpdateException` wrapping a Postgres `23505`), and the losing
     request simply re-queries and returns the winner's row instead of
     erroring. The database's unique index is the actual arbiter here, not a
     lock or a retry-until-it-works loop in application code.

## Why controllers, not Minimal APIs

Controllers were used over Minimal API endpoints because it's the setup I'm
already familiar with from prior ASP.NET work, and I find it clearer -
`ControllerBase`'s helpers (`Problem()`, `Ok()`, `Created()`, `Redirect()`)
and attribute routing read more explicitly than Minimal API's route-plus-lambda
style. Either would work for an API this size; this is a familiarity/clarity
preference, not a technical requirement of the project.

## Assumptions and design choices

- **Status lifecycle** (`lt_status`: `initializing` / `active` / `archived`)
  exists because the short code can't be known until after the row is
  inserted (it's derived from the DB-generated `mapping_id`). A row is briefly
  `initializing` (no short code yet) between the insert and the follow-up
  update. `archived` isn't produced by any current code path - it's there for
  a future "disable a short URL" feature. `1` is deliberately unused as a
  status value, to avoid it reading like a boolean.
- **`date_last_used`** is tracked on every successful resolve, even though
  it's not required by the assignment - cheap to add, useful for seeing which
  links are actually live.
- **`Base62`, `UrlValidator`, `UrlNormalizer` are static classes**, not
  DI-injected services. The assignment says "no static service classes" -
  these aren't business-logic services, they're pure functions (same input,
  same output, no state, no I/O). `UrlShorteningService`, which does the
  actual orchestration and DB access, is a normal DI-registered class.
- **No EF Core migrations** - the schema is hand-written SQL (above), and
  `AppDbContext` is configured to match it exactly (explicit column names,
  types, the partial unique index). This was a deliberate choice to stay in
  direct control of the schema, rather than letting
  migrations generate it.
- **Validation length check runs against both the raw input and the
  canonical (fully-escaped) URL.** A URL can look like it's under the 2048
  character limit as typed, but expand once properly percent-encoded. Both
  are checked rather than only the raw string.
- **`IsWellFormedOriginalString()`** rejects things like a raw, unescaped
  space in the URL - `Uri.TryCreate` alone would silently "fix" that by
  escaping it, which isn't the same as the input actually being valid.

## What's left / what I'd improve with more time

- **Dockerfile / docker-compose** for the app itself - not done yet.
- A crash between the two save steps in `GetOrCreateAsync` (insert, then
  assign the short code) would leave a row permanently stuck in
  `initializing` with no short code. Rare in practice, not currently cleaned
  up or retried.
- No auth/rate limiting on the shorten endpoint - out of scope for this
  assignment, but anything public-facing would need it.
- The `TestController` endpoints (`/api/v1/urls/test/...`) are debug-only
  scratch endpoints used while building `Base62`/`UrlValidator` - not part of
  the assignment's API contract, and would be removed (or gated out of
  Production) before any real deployment.
