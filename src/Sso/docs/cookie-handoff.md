# Cookie Handoff

The handoff has two separate mechanisms working together — one gets the cookie to the browser for both hosts, the other lets `RelyingApp` make sense of a cookie it never wrote.

## 1. Getting the browser to send the cookie to both apps

`IdentityProvider` sets the cookie's domain to the shared parent, not its own host:

```csharp
o.Cookie.Domain = sso.CookieDomain;   // ".authr-sso.local"
```
(`IdentityProvider/Program.cs:23`)

A cookie scoped to `.authr-sso.local` is sent by the browser on requests to *any* subdomain — `identity.authr-sso.local` and `app.authr-sso.local` alike. `RelyingApp` never sets `Cookie.Domain` at all (`RelyingApp/Program.cs:18`), so it just inherits whatever the browser hands it. That's the entire "SSO" part: one cookie, two hostnames, no code coupling.

## 2. Letting `RelyingApp` read a cookie it never issued

ASP.NET Core cookie auth doesn't store the ClaimsPrincipal in the cookie as plaintext — it encrypts it via Data Protection, keyed off an application name and a key ring. Normally each app has its own auto-generated key ring, so app B can't decrypt app A's cookie even if the browser sends it.

Both apps point at the *same* key ring folder and the *same* application name:

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName(sso.KeyRingApplicationName);   // "authr-sso-demo" for both
```
(`IdentityProvider/Program.cs:31-33`, `RelyingApp/Program.cs:37-39`)

So when `RelyingApp` gets a cookie stamped by `IdentityProvider`, it derives the same decryption key and reads the same `ClaimsPrincipal` straight out of it.

## The actual round trip

1. Browser hits `RelyingApp` `/secret` with no cookie → `[Authorize]` fails → `OnRedirectToLogin` fires (`RelyingApp/Program.cs:23-29`), which redirects to `IdentityProvider`'s `/login?returnUrl=<the original url, escaped>` instead of a local login page (there isn't one).
2. `IdentityProvider/login` builds a `ClaimsPrincipal` and calls `SignInAsync` (`IdentityProvider/Program.cs:44-51`) — this writes the `.authr-sso.local`-scoped cookie, encrypted with the shared key ring.
3. It redirects back to `returnUrl`, but only after `sso.IsTrustedReturnUrl(returnUrl)` checks the host is within `CookieDomain` — otherwise it falls back to `/`, closing the open-redirect hole.
4. Browser follows the redirect to `app.authr-sso.local/secret`, attaching the cookie automatically (step 1's domain scoping) — no app-to-app call happened.
5. `RelyingApp`'s cookie middleware decrypts it (step 2's shared key ring) and populates `ctx.User` — `[Authorize]` now passes.

Logout is the same trick in reverse: `IdentityProvider/logout` calls `SignOutAsync`, which clears the shared cookie — since both apps were reading the same cookie, `RelyingApp` is logged out too without ever being told.
