# URL Shortener Microservice (.NET Core)

## Overview

Build a small, production-minded URL shortener microservice in .NET (ASP.NET Core). The service accepts a long URL and returns a short URL; submitting the same long URL again must return the same short URL, not a new one.

We use this assignment to see how you design an API, model data, handle edge cases and write tests. We value clean, readable, well-reasoned code over a large number of features.

## Functional requirements

The service must expose two endpoints: one to create a short URL and one to resolve it.

### Shorten a URL

Accept a long URL and return a short URL built from a unique short code (e.g. `https://sho.rt/aZ3kQ9x`).

- The short code must be 6–10 characters, URL-safe (Base62: a-z, A-Z, 0-9).
- Codes must be unique; collisions must be detected and handled.
- **Idempotency** — same long URL, same short URL. If a long URL that already exists is submitted again, return the existing short URL. Do not create a duplicate record.
- Normalize the URL before comparing: trim whitespace, lowercase the scheme and host, and remove a trailing slash on the root path. Document any other normalization you choose.
- The guarantee must hold under concurrent requests: two simultaneous requests with the same URL must receive the same short code.

### Resolve a short URL

- `GET /{code}` redirects the caller to the original long URL. An unknown code returns 404.

### Validation

Reject input that is empty, not an absolute http/https URL, or longer than 2,048 characters, with a clear error message.

### Persistence

Mappings must survive a service restart.

## API contract

Implement the endpoints below. You may extend the response with extra fields, but keep these shapes.

### `POST /api/v1/urls` — create a short URL

Request:

```json
{
  "longUrl": "https://www.example.com/products/category/shoes?color=red&size=42"
}
```

Response (first time this URL is submitted) — `201 Created`, with a `Location` header:

```json
{
  "shortCode": "aZ3kQ9x",
  "shortUrl": "https://sho.rt/aZ3kQ9x",
  "longUrl": "https://www.example.com/products/category/shoes?color=red&size=42",
  "createdAt": "2026-10-21T09:15:00Z"
}
```

Response (same URL submitted again) — `200 OK` with the same `shortCode` and `shortUrl` as the first call.

### `GET /{shortCode}` — resolve

Returns `302 Found` with `Location: <longUrl>`.

## Status codes

| Case | Status |
|---|---|
| New short URL created | 201 Created |
| Existing mapping returned for a repeated long URL | 200 OK |
| Redirect to the long URL | 302 Found |
| Invalid or missing URL in request | 400 Bad Request (ProblemDetails body) |
| Unknown short code | 404 Not Found |
| Unexpected error | 500 Internal Server Error (no stack trace in body) |

Errors must use the standard ProblemDetails format (RFC 9457).

## Technical requirements

- **Framework:** .NET 8 (LTS) or later, ASP.NET Core Web API (controllers or Minimal APIs — your choice, justify it in the README).
- **Structure:** clear separation between API, application logic and data access (e.g. layered or clean architecture). Use dependency injection; no static service classes.
- **Storage:** any persistent store — SQL Server, PostgreSQL or SQLite via EF Core, or Redis. A unique index on the normalized long URL (or an equivalent mechanism) must enforce idempotency at the database level, not only in code.
- **Short-code generation:** choose and explain your strategy (e.g. Base62 of a sequence ID, random code with collision retry, or hash-based). Explain its trade-offs in the README.
- **Configuration:** base short domain (e.g. `https://sho.rt`) and connection strings come from `appsettings.json` / environment variables, not hard-coded.
- **Documentation:** Swagger/OpenAPI UI available in Development.
- **Logging & health:** structured logging via `ILogger`, and a `/health` endpoint.
- **Tests:**
  - Unit tests for code generation, URL validation and normalization (xUnit or NUnit).
  - Integration tests using `WebApplicationFactory` that cover: create, repeated create returns the same short URL, redirect, invalid input and unknown code.
- **Run:** a Dockerfile (and `docker-compose.yml` if you use an external database) so we can start everything with one command.

## Deliverables and submission

Submit a link to a Git repository (GitHub, GitLab or Azure DevOps) containing:

- Source code with a meaningful commit history (not one single commit).
- Unit and integration tests that pass with `dotnet test`.
- A `README.md` that covers:
  - How to build, run and test the service.
  - Example curl requests for each endpoint.
  - Your short-code strategy and how idempotency is guaranteed, including under concurrency.
  - Assumptions, trade-offs and what you would improve with more time.

Make the repository accessible to the reviewers before the deadline.
