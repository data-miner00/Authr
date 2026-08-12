# Single Sign-On via Shared Cookie

Two independent apps, one login. Signing in at `IdentityProvider` also authenticates you at
`RelyingApp` - without either app calling the other, and without any token exchange.

## Projects

- **IdentityProvider**: Owns `/login`. Issues a cookie scoped to the shared parent domain.
- **RelyingApp**: Never issues a cookie of its own. It only ever reads the one `IdentityProvider` wrote, and redirects to `IdentityProvider` when there isn't one yet.

## The mechanism

SSO here rests on two settings that must match exactly between the two apps:

1. **A shared cookie domain.** `IdentityProvider` sets `o.Cookie.Domain = ".authr-sso.local"`. A cookie scoped to a parent domain is sent by the browser to every subdomain under it, so `RelyingApp` receives it automatically on the next request - no redirect-with-token needed to hand it over.
2. **A shared Data Protection key ring.** Both apps call `AddDataProtection().PersistKeysToFileSystem(...).SetApplicationName("authr-sso-demo")` against the *same* folder and application name. ASP.NET Core cookie auth encrypts the ticket with a key derived from that ring; pointing both apps at the same one is what lets `RelyingApp` decrypt a cookie it never issued.

Everything else in the code exists to make that handoff visible: `RelyingApp` has no login page, so its `OnRedirectToLogin` event sends unauthenticated visitors to `IdentityProvider/login?returnUrl=...` instead; `IdentityProvider` signs them in and redirects back, at which point the shared cookie is already valid at both hosts.

## Running it locally

The shared-domain trick needs two real subdomains, so map them to loopback first:

```
127.0.0.1 identity.authr-sso.local
127.0.0.1 app.authr-sso.local
```

(On Windows: `C:\Windows\System32\drivers\etc\hosts`, as Administrator.)

Then run both projects (`dotnet run` in each folder, or the `Sso.slnx` solution with both set as startup projects). Visit `http://app.authr-sso.local:5302/secret` - `RelyingApp` will bounce you to `IdentityProvider` to sign in, then bounce you straight back in, authenticated.

Optionally, `Caddyfile` reverse-proxies both hosts to their `dotnet run` ports so you can drop the port number from the URL (`caddy run` from this folder). Still needs the hosts-file mapping above - Caddy answers on those hostnames, it doesn't make them resolve.

HTTPS is intentionally not configured: the ASP.NET Core dev certificate is issued for `localhost`, not `*.authr-sso.local`, so enabling it would just trade a real demo for a certificate-warning demo.

## Endpoints

| App | Route | Notes |
| --- | --- | --- |
| IdentityProvider | `GET /login?returnUrl=` | Signs in a demo user, redirects to `returnUrl` only if its host is within `CookieDomain` |
| IdentityProvider | `GET /logout` | Signs out - since the cookie is shared, this ends the session everywhere, not just here |
| IdentityProvider | `GET /whoami` | Requires auth; dumps the caller's claims |
| RelyingApp | `GET /secret` | Requires auth; redirects to IdentityProvider if not yet signed in |
| RelyingApp | `GET /` | Reports whether the shared cookie is currently authenticated |

