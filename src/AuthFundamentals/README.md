# Auth Fundamentals

The building blocks everything else in this repo specializes: how a cookie becomes an identity,
and how an identity becomes a yes/no decision. Two independent projects, one progression each -
every stage in a project does the same thing, each one automating more of the last.

## Projects

- **Authentication**: four ways to turn a login into "the caller is recognized on the next
  request" - from a raw `Set-Cookie` header to the built-in cookie scheme.
- **Authorization**: four ways to turn an identity into an allow/deny decision - from hand-rolled
  checks to a policy backed by a custom requirement.

They're independent processes; run whichever one you want to explore.

## Authentication endpoints

Each stage owns its own cookie name and its own `login`/`whoami` pair, so stages don't interfere
with each other.

| Stage | Route | What it demonstrates |
| --- | --- | --- |
| Raw cookie | `GET /raw-cookie/login`, `GET /raw-cookie/whoami` | A plaintext `Set-Cookie` header, read back by hand. No encryption, no scheme. |
| Encrypted cookie | `GET /encrypted-cookie/login`, `GET /encrypted-cookie/whoami` | Same shape, but the value is run through `IDataProtectionProvider` before it ever reaches the browser. |
| Claims middleware | `GET /claims-middleware/login`, `GET /claims-middleware/whoami` | A hand-written middleware decrypts the cookie and builds a `ClaimsIdentity`/`ClaimsPrincipal` itself, assigning `ctx.User`. |
| Built-in cookie scheme | `GET /built-in/login`, `GET /built-in/whoami` | `AddAuthentication().AddCookie()` + `SignInAsync` - the same outcome as the three stages above, for a fraction of the code. |

## Authorization endpoints

A single `POST`-free `GET /login` signs in a demo user (`passport: eur`, born ~19 years ago) via
the built-in cookie scheme - authorization is the point here, not re-demonstrating authentication.

| Stage | Route | What it demonstrates |
| --- | --- | --- |
| Manual | `GET /manual/secret` | Authentication and authorization both hand-checked inside the endpoint. |
| Middleware | `GET /middleware/secret` | The same two checks, but centralized in one piece of middleware ahead of every route under `/middleware`. |
| Policy | `GET /policy/secret` | A built-in `AuthorizationPolicy` (`RequireAuthenticatedUser` + `RequireClaim`) applied via `.RequireAuthorization(...)`. |
| Custom requirement | `GET /custom-requirement/secret` | The policy extended with a custom `IAuthorizationRequirement`/`AuthorizationHandler<T>` pair (`MinimumAgeRequirement`, 18+). |

## What changed from the original

Restored and refactored from the root-level `Authr.WebApi` project (recovered from
`a13c8ff~1:Authr.WebApi`, removed in `a13c8ff chore: Remove old and confusing projects`), which
crammed both topics and all eight techniques into one flat `Program.cs`.

- **Split into two projects.** Authentication and authorization are unrelated concerns; the
  original coupled them only because they lived in one file. Each stage within a project also got
  its own `Endpoints/` file, following the pattern already used in
  `src/JwtAuthentication/WeatherApi`.
- **Fixed the original's own documented bug.** Its `/sweden` endpoint had a comment noting it
  "returns 404 for unauthorized request instead of 401" and suspected a bad redirect URI. That's
  exactly right: the cookie scheme's default `ForbidAsync`/`ChallengeAsync` redirect to
  `/Account/AccessDenied` and `/Account/Login`, which don't exist in a minimal API project. The
  `Authorization` project's cookie scheme now overrides `OnRedirectToLogin`/`OnRedirectToAccessDenied`
  to return plain 401/403 instead, and the manual stage avoids `Results.Forbid()` for the same
  reason.
- **Added a real middleware-based authorization stage.** The original's README described a global
  `app.Use` doing authentication/authorization checks, but never actually implemented it - it was
  pseudocode only. `MiddlewareAuthorizationEndpoints` builds a real (path-scoped, not truly global)
  version of it.
- **Dropped the "AuthService" idea.** The original README floats centralizing the auth logic into
  an injectable service, but no such class ever existed in the actual project - there was no
  working code to restore. Worth revisiting as a follow-up refactor once there's more than one
  consumer of the same logic to justify it.
- **Dropped attribute-based authorization.** Also pseudocode-only in the original
  (`[AuthScheme(...)]`/`[AuthClaim(...)]` aren't real attributes - a working version needs MVC
  filter infrastructure, not minimal APIs). A real version of this concept already exists in this
  repo at `src/JwtAuthentication/WeatherApi/Attributes/RequiresClaimAttribute.cs`
  (an `IAuthorizationFilter`); restoring it here would just duplicate that.
- **Dropped the unused second cookie scheme.** The original registered a `"cookie2"` scheme that
  no endpoint ever used. Multiple cooperating schemes is its own concept, already queued as a
  future restoration (`Authr.Cookie.Schemes`) - building it here too would duplicate that work.
