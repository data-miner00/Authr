# Progress

Working log for the "learn auth in C#, one concept per `src/` solution" effort. Not a durable doc —
prune or fold sections into per-project READMEs once they stop being current.

## Where things stand

- Repo used to be flat (projects at root). That layout was scrapped on 2026-08-11
  (`a13c8ff chore: Remove old and confusing projects`) because it had become hard to read back.
- New convention: each concept gets its own folder under `src/`, with its own `.slnx`/`.sln`,
  its own `README.md`, and role-named project folders (not `Authr.*`-prefixed).
- Current concepts under `src/`: `Cookies` (Mvc, Spa), `JwtAuthentication` (Core, IdentityApi,
  WeatherApi), `MultiFactorAuth` (TimeBasedOTP), `OAuth2/ClientCredentials` (AuthServer,
  ResourceApi, WebApi — mid-scaffold per `31aa981 wip: Scaffold resource and web api for client
  credentials`), `SimpleBookApi`, `Sso` (`2b3a6dd feat: Resurrect Sso projects`), and now
  `AuthFundamentals` (see below).

## This session

1. **Salvaged the deleted root projects.** Recovered all 9 pre-`a13c8ff` projects from git
   history (`f3c2c60`), read every `Program.cs`/README, and ranked the concepts each one
   illustrated by relevance and how worth revisiting they are. Published as an artifact:
   https://claude.ai/code/artifact/e8d8bc20-5b02-4de4-8d80-cdd48875da81

   Ranked (relevance / revisit-interest out of 10):
   1. `Authr.WebApi` (10/10) — the master narrative: raw cookies → DataProtection encryption →
      ClaimsPrincipal → built-in cookie scheme → manual/attribute/middleware/policy authorization
      → custom `AuthorizationHandler`. **Restore this next** — it's the outline everything else
      specializes.
   2. `Authr.Cookie.Sso.App` + `.Identity` (9/9) — cross-app SSO via shared DataProtection key
      ring + shared cookie domain. **Done this session**, see below.
   3. `Authr.Cookie.Schemes` (8/8) — multiple cookie schemes cooperating, plus a custom
      `CookieAuthenticationHandler` subclass that auto-signs-in strangers as an anonymous
      "visitor" on failed auth. Not yet restored.
   4. `Authr.IdentityMgmt.WebApi` (7/7) — mostly scaffolding, but the password-reset-via-
      `IDataProtectionProvider` token pattern (no token table needed) is worth lifting out on
      its own. Not yet restored.
   5. `Authr.Roles` (7/6) — "roles are just claims" proven via the `roleType` ctor param on
      `ClaimsIdentity`. Not yet restored.
   6. `Authr.Roles.IF` (5/4) — EF Core Identity + roles. Likely redundant with
      `src/JwtAuthentication/IdentityApi` — diff before reviving.
   7. `Authr.Core` + `Authr.Core.UnitTests` (3/3) — a generic `Guard` clause library that wasn't
      really about auth. Low priority, probably stays dead.

2. **Restored + refactored #2 (SSO pair) as `src/Sso/`.** New solution `Sso.slnx` with two
   projects:
   - `IdentityProvider` — issues the cookie, scoped to `.authr-sso.local`.
   - `RelyingApp` — never issues its own cookie, only reads the shared one; redirects to
     `IdentityProvider` via a custom `OnRedirectToLogin` handler when unauthenticated.

   Fixed relative to the salvaged original: hardcoded personal key-ring path
   (`C:\Users\User\Downloads`) → relative `../.keyring` (gitignored); no way to actually exercise
   the handoff (each app 401'd in isolation) → a real click-through login redirect flow; no
   `returnUrl` validation → `SsoOptions.IsTrustedReturnUrl` allow-lists by cookie-domain suffix
   to avoid an open redirect.

   Verified end-to-end with both apps running and `curl --resolve` (no hosts-file edit made —
   that's still an unapplied step, see below): unauthenticated hit on `/secret` → redirect to
   `IdentityProvider` → sign-in → redirect back → authenticated at `RelyingApp`; untrusted
   `returnUrl` correctly falls back to `/`; logout at `IdentityProvider` kills the session at
   `RelyingApp` too (same cookie). `dotnet build src/Sso/Sso.slnx` is clean.

   Full writeup: `src/Sso/README.md`. Committed as `2b3a6dd feat: Resurrect Sso projects`.
   `Models` folders in both projects were later renamed to `Options` (user's naming convention
   for options-pattern classes) — namespaces and `using`s updated to match, build re-verified.

3. **Relocated the root `Caddyfile` into `src/Sso/`.** Confirmed via `git log --follow` it was
   added in the same commit (`6bab568`) as the original `Authr.Cookie.Sso.App`/`.Identity` pair,
   so it belongs to this project, not the repo root. It was stale (`company.local` on
   5142/5067) relative to the restored project's actual domain/ports (`authr-sso.local` on
   5301/5302) — updated both host blocks to match `launchSettings.json`, and added a short
   optional-use note to `src/Sso/README.md`. Also added `src/Sso/docs/cookie-handoff.md`, a
   deeper writeup of the shared-domain + shared-key-ring mechanism, with `Program.cs` line
   references.

4. **Restored + refactored #1 (`Authr.WebApi`) as `src/AuthFundamentals/`.** Per user request,
   segregated into two independent projects rather than the original's single flat `Program.cs`:
   - `Authentication` — four cookie-auth stages, each its own `Endpoints/*.cs` file with its own
     route prefix/cookie name: `RawCookieEndpoints` (plaintext header) → `EncryptedCookieEndpoints`
     (DataProtection) → `ClaimsMiddlewareEndpoints` (hand-built `ClaimsPrincipal` in middleware) →
     `BuiltInCookieEndpoints` (`AddCookie()` + `SignInAsync`).
   - `Authorization` — shared `/login`, then four stages: `ManualAuthorizationEndpoints` (in-
     handler checks) → `MiddlewareAuthorizationEndpoints` (centralized, path-scoped — this one
     was pseudocode-only in the original, built for real here) → `PolicyAuthorizationEndpoints`
     (built-in policy) → `CustomRequirementAuthorizationEndpoints` (ported `MinimumAgeRequirement`
     /`MinimumAgeHandler`).

   Deliberately dropped from the original: the "AuthService" idea (never had real code), attribute-
   based authorization (also pseudocode, and a real version already exists at
   `src/JwtAuthentication/WeatherApi/Attributes/RequiresClaimAttribute.cs`), and the unused second
   cookie scheme (`"cookie2"` — that concept is `Authr.Cookie.Schemes`, still queued below).

   Fixed the original's own documented bug: its `/sweden` endpoint had a comment noting it
   "returns 404 instead of 401" — root cause was the cookie scheme's default `ForbidAsync`/
   `ChallengeAsync` redirecting to `/Account/AccessDenied`/`/Account/Login`, which don't exist in
   a minimal API. Both new projects override `OnRedirectToLogin`/`OnRedirectToAccessDenied` to
   return plain 401/403 instead.

   Verified end-to-end with both apps running and `curl`: all four `Authentication` stages
   round-trip to the same identity (confirmed the raw-cookie value is plaintext, the encrypted one
   isn't); all four `Authorization` stages return 200 when signed in and 401 when not. Caught and
   fixed one real ordering bug in the process — `MiddlewareAuthorizationEndpoints`'s custom
   middleware read `ctx.User` before `UseAuthentication()` had populated it, always 401ing; fixed
   by reordering `Program.cs` so `UseAuthentication()` runs first. `dotnet build
   src/AuthFundamentals/AuthFundamentals.slnx` is clean. Full writeup: `src/AuthFundamentals/README.md`.

   **Not committed** — sitting as untracked `src/AuthFundamentals/`.

## Open items / next steps

- **Commit the `AuthFundamentals` work** (untracked) — waiting on the user to say go.
- **To run `Sso` in a browser**, `identity.authr-sso.local` and `app.authr-sso.local` need to be
  mapped to `127.0.0.1` in the hosts file (`C:\Windows\System32\drivers\etc\hosts`, as
  Administrator). Not done — was deliberately left to the user rather than editing a system file
  unasked.
- Next candidate to restore, per the ranking above: `Authr.Cookie.Schemes` (#3, multiple
  cooperating cookie schemes + a custom `CookieAuthenticationHandler` subclass).
- `Authr.Roles.IF` should be diffed against `src/JwtAuthentication/IdentityApi` before deciding
  whether it's worth restoring at all.
