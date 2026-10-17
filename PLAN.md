# Plan

Durable roadmap for the "learn auth in C#, one concept per `src/` solution" effort. Unlike
`PROGRESS.md` (a working log, pruned/rotated as sessions end), this file is the map: what's done,
what's next, and why the ordering is what it is. Update it when priorities shift; don't log
session-by-session narration here — that belongs in `PROGRESS.md`.

## Conventions (already established, keep following them)

- One concept per folder under `src/`, its own `.slnx`/`.sln`, its own `README.md`.
- Projects inside a concept are role-named (`IdentityProvider`, `ResourceApi`, ...), never
  `Authr.*`-prefixed.
- Options-pattern classes live in an `Options/` folder (not `Models/`).
- "Done" means: builds clean (`dotnet build <slnx>`), verified end-to-end with the apps actually
  running (curl or browser — not just "it compiles"), and a `README.md` that explains the
  mechanism, not just lists routes.
- Every concept should leave behind one thing worth remembering that isn't obvious from the
  ASP.NET Core docs alone — a gotcha, a bug it's easy to fall into, a "here's the actual bytes on
  the wire" moment. `src/Sso/docs/cookie-handoff.md` and the `/sweden` 404-vs-401 fix in
  `AuthFundamentals` are the bar.

## Status snapshot (as of 2026-08-13)

| Concept | Folder | State |
| --- | --- | --- |
| Cookie auth fundamentals (raw → encrypted → claims middleware → built-in scheme) + authorization (manual → middleware → policy → custom requirement) | `src/AuthFundamentals` | Built, verified, **not committed yet** |
| JWT issuance + validation, refresh tokens | `src/JwtAuthentication` | Done |
| TOTP-based MFA | `src/MultiFactorAuth/TimeBasedOTP` | Done |
| Cross-app SSO via shared cookie domain + shared Data Protection key ring | `src/Sso` | Done (needs hosts-file edit to click through in a browser) |
| OAuth2 client credentials flow | `src/OAuth2/ClientCredentials` | Mid-scaffold (`AuthServer`, `ResourceApi`, `WebApi` exist; not wired end-to-end) |
| Cookie-based session basics (MVC + SPA) | `src/Cookies` | Done, predates this convention — low priority to revisit |
| — | `src/SimpleBookApi` | Empty placeholder, unclear purpose — decide keep or delete |

Salvaged-but-not-yet-restored concepts (full ranking/rationale in `PROGRESS.md`'s session-1
artifact link):

| Original project | Concept | Priority |
| --- | --- | --- |
| `Authr.Cookie.Schemes` | Multiple cooperating cookie schemes + custom `CookieAuthenticationHandler` subclass | High |
| `Authr.IdentityMgmt.WebApi` | Password reset via `IDataProtectionProvider` token (no token table) | Medium |
| `Authr.Roles` | Roles-are-just-claims, via `ClaimsIdentity`'s `roleType` ctor param | Medium |
| `Authr.Roles.IF` | EF Core Identity + roles | Low — diff against `src/JwtAuthentication/IdentityApi` first, likely redundant |

## Phase 0 — Close out what's in flight

Don't start new concepts with two unfinished ones sitting in the tree.

1. Commit `src/AuthFundamentals` (currently untracked — waiting on explicit go-ahead per
   `PROGRESS.md`).
2. Finish wiring `src/OAuth2/ClientCredentials` end-to-end: `AuthServer` issues a token,
   `WebApi`/`ResourceApi` validates it and actually rejects unauthenticated/wrong-scope calls,
   README documents the flow with a real curl transcript (matching the bar set by
   `AuthFundamentals`/`Sso`). Right now it's scaffolding with no verified round trip.
3. Map `identity.authr-sso.local` / `app.authr-sso.local` to loopback in the hosts file so `Sso`
   can actually be clicked through in a browser, not just curled.
4. Decide the fate of `src/SimpleBookApi` (empty) — fold into a real concept or delete it.

## Phase 1 — Finish the salvage list

Everything here already has working reference code from the old flat layout; it's a restore +
refactor job, not new research. Order follows the priority column above.

5. **`src/Cookies/Schemes`** (or similar) — two named cookie schemes registered side by side
   (`AddAuthentication().AddCookie("A").AddCookie("B")`), a custom
   `CookieAuthenticationHandler` subclass, and the "auto-sign-in strangers as anonymous visitor on
   failed auth" trick from the original. This is the natural sequel to `AuthFundamentals` — it's
   the thing that makes `[Authorize(AuthenticationSchemes = "...")]` make sense.
6. **`src/PasswordReset`** (or fold into `src/JwtAuthentication`) — token-less password reset:
   `IDataProtectionProvider.Protect`/`Unprotect` on a payload (user id + expiry) instead of a
   token table. Worth calling out explicitly why this trades a DB round-trip for "can't revoke a
   single token early."
7. **`src/Roles`** — prove roles are just claims by passing a custom `roleType` into
   `ClaimsIdentity`, then show the same outcome via ASP.NET Core Identity's `IdentityRole` for
   contrast.
8. Diff `Authr.Roles.IF` against `src/JwtAuthentication/IdentityApi` before deciding whether it's
   worth restoring — likely redundant, don't restore it just to restore it.

## Phase 2 — The rest of OAuth2

Client credentials (machine-to-machine, no user) is the simplest grant and already underway. The
other grants are where most real-world confusion lives — each one earns its own folder under
`src/OAuth2/`.

9. **`Authr.OAuth2.Code` (Authorization Code + PKCE)** — the flow every "Login with X" button
   actually uses. Build a minimal in-house AuthServer (reuse/extend the one from
   `ClientCredentials`) plus a client that redirects, handles the callback, and exchanges the code
   for a token with a `code_verifier`. The teaching point: PKCE exists because a public client
   (SPA/mobile) can't hold a client secret.
10. **`Authr.OAuth2.RefreshRotation`** — refresh token rotation and reuse detection (a stolen
    refresh token used after the legitimate client already rotated it should kill the whole
    token family). `src/JwtAuthentication/Core/RefreshToken.cs` already has a basic refresh token
    — this phase is specifically about the *rotation* and *reuse-detection* semantics, not just
    issuing a longer-lived token.
11. **`Authr.OAuth2.DeviceCode`** — device authorization grant (the "go to this URL and enter this
    code" flow smart TVs/CLIs use). Small, but a genuinely different shape (polling instead of
    redirect).
12. Skim the Resource Owner Password Credentials grant just long enough to write down *why it's
    deprecated* (app sees the raw password) — not worth a full project.

## Phase 3 — OpenID Connect

OIDC is a thin identity layer on top of OAuth2's authorization code flow — doing this before
understanding Phase 2 would blur the line between "who are you" (OIDC) and "what can you do"
(OAuth2), which is exactly the confusion most tutorials leave people with.

13. **`Authr.Oidc`** — extend the Phase 2 authorization-code AuthServer to emit an `id_token`
    (JWT with `iss`/`aud`/`sub`/`nonce`), expose a discovery document
    (`/.well-known/openid-configuration`) and `/userinfo`. Validate the `id_token` against the
    [OIDC Debugger](https://oidcdebugger.com/) (already in the repo's tool list) as an external
    sanity check, not just self-validation.
14. Write down the one-paragraph version of "OAuth2 answers *can this app act on your behalf*;
    OIDC answers *who is this*" with the concrete evidence from steps 9 and 13 side by side
    (access token vs id_token, what each is allowed to be used for).

## Phase 4 — JWT deep dive

`src/JwtAuthentication` already covers issuance/validation with a single static signing key. This
phase is what breaks in production that a tutorial single-key setup never surfaces.

15. **JWKS + key rotation** — move from a static symmetric key to RS256 with a `/​.well-known/
    jwks.json` endpoint (the client credentials AuthServer already has `KeysHelper.cs` /
    `AuthServer.pem` — reuse that). Rotate keys with overlapping validity and confirm old tokens
    still validate until they expire, new ones use the new key immediately.
16. **Revocation strategies** — since JWTs are stateless by design, demonstrate the three real
    options side by side: short expiry + refresh (already have the pieces), a denylist/jti
    check against a store, or a `not-before` claim tied to a per-user "logout everywhere"
    timestamp. Write down the trade-off each one makes.

## Phase 5 — Federation & external identity

17. **Social login** — wire up one real external OIDC provider (Google/Microsoft) as a client,
    not a fake one. This is Phase 3's client role played against a real IdP instead of the
    in-house one — reuse the same code path, swap the provider.
18. **SAML** — a minimal SP that consumes an assertion, checked against the
    [SAML Tools](https://www.samltool.com) site already listed in the root README. Lower
    priority than OIDC (SAML is legacy-enterprise territory) but worth one project for the
    contrast: XML + assertions + no bearer token, vs OIDC's JWT + bearer token.

## Phase 6 — Modern factors & session hardening

19. **WebAuthn/FIDO2 (passkeys)** — register + authenticate with a platform authenticator.
    Genuinely different trust model from everything above (no shared secret at all).
20. **MFA beyond TOTP** — recovery codes (what happens when the authenticator is lost) and
    step-up auth (require a fresh MFA challenge for a sensitive action on an already-authenticated
    session).
21. **Session hardening as a cross-cutting pass over everything already built**: CSRF defenses on
    cookie-authenticated endpoints, token storage guidance (why not localStorage), session
    fixation (rotate the session/cookie on privilege change, e.g. post-login), login
    brute-force/lockout, secure logout (does it actually invalidate server-side state, or just
    clear a cookie?). This is a "go back and audit" phase, not new projects.

## Phase 7 — Service-to-service & infra

22. **mTLS** — client-cert auth between two services, contrasted with the bearer-token model used
    everywhere else in the repo.
23. **API keys vs tokens** — when a static API key is the right call (and why it usually isn't
    for anything user-facing).
24. **Reverse-proxy/gateway auth** — Caddy is already in the repo (`src/Sso/Caddyfile`) and in the
    root README's tech list; use it to demonstrate auth enforced at the edge (forward-auth) vs
    in-app, and the trade-offs.

## Phase 8 — Authorization models beyond RBAC/policy

`AuthFundamentals` already covers claims-based and policy-based (including a custom requirement).
This phase is the next tier up.

25. **Resource-based authorization** — `IAuthorizationService.AuthorizeAsync(user, resource,
    requirement)`, where the decision depends on the specific resource instance (e.g. "can this
    user edit *this* document"), not just the user's claims.
26. **ABAC/PBAC** — attribute-based decisions (time of day, resource sensitivity, request
    context) as a generalization of the policy work already done.
27. **Multi-tenant scoping** — same user, different permissions per tenant; where the tenant id
    lives (claim vs route vs header) and how it gets enforced without leaking across tenants.

## Suggested working order

Phase 0 is not optional — finish it before anything else. After that, Phases 1–4 build on each
other directly (cookies → OAuth2 → OIDC → JWT internals) and should stay in order. Phases 5–8 are
comparatively independent of each other; pick by curiosity/relevance once Phase 4 is done.

## Open questions to resolve along the way

- Does `src/Cookies` get folded into or superseded by `AuthFundamentals`, or does it stay as-is
  for the MVC/SPA framing specifically? Decide before Phase 6's "audit everything" pass, so there's
  no ambiguity about what's still in scope.
- `Authr.Roles.IF` — restore or drop (Phase 1, step 8).
- `src/SimpleBookApi` — restore purpose or delete (Phase 0, step 4).
