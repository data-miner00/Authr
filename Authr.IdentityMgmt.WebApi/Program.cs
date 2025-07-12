using System.Security.Claims;
using Authr.IdentityMgmt.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddDataProtection();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("manager", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            .RequireClaim("role", "manager");
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/", () => "hello world");

app.MapGet("/register", async (
    string username,
    string password,
    Database db,
    IPasswordHasher<User> hasher,
    HttpContext ctx) =>
{
    var user = new User
    {
        Username = username,
    };
    user.HashedPassword = hasher.HashPassword(user, password);
    await db.PutAsync(user);

    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user));

    return user;
});

app.MapGet("/login", async (
    string username,
    string password,
    Database db,
    IPasswordHasher<User> hasher,
    HttpContext ctx) =>
{
    var user = await db.GetUserAsync(username);
    var result = hasher.VerifyHashedPassword(user, user.HashedPassword, password);

    if (result == PasswordVerificationResult.Failed)
    {
        return "bad credentials";
    }

    await ctx.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        UserHelper.Convert(user));

    return "logined";
});

app.MapGet("/promote", async (string username, Database db) =>
{
    var user = await db.GetUserAsync(username);

    user.Claims.Add(new Claim("role", "manager"));
    await db.PutAsync(user);
    return "promoted";
});

app.MapGet("/start-reset-pw", async Task<string> (string username, Database db, IDataProtectionProvider provider) =>
{
    var protector = provider.CreateProtector("PasswordReset");
    var user = await db.GetUserAsync(username);
    return protector.Protect(user.Username);
});

app.MapGet("/end-reset-pw", async Task<string> (
    string username,
    string password,
    string hash,
    Database db,
    IPasswordHasher<User> hasher,
    IDataProtectionProvider provider) =>
{
    var protector = provider.CreateProtector("PasswordReset");
    var hashUsername = protector.Unprotect(hash);
    if (hashUsername != username)
    {
        return "bad hash";
    }

    var user = await db.GetUserAsync(username);
    user.HashedPassword = hasher.HashPassword(user, password);

    await db.PutAsync(user);

    return "password reset";
});

app.MapGet("/protected", () => "something super secret").RequireAuthorization("manager");

app.MapGet("/test", (UserManager<IdentityUser> userMgr, SignInManager<IdentityUser> signInMgr) =>
{
    // can access to userMgr and signInMgr
});

app.Run();
