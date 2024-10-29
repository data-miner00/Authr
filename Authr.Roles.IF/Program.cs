namespace Authr.Roles.IF;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public static class Program
{
    public const string ComplicatedPassword = "AbcdEFgHIJk123!";

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<IdentityDbContext>(ctx => ctx.UseInMemoryDatabase("db"));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
        {
            o.User.RequireUniqueEmail = true;

            o.Password.RequireDigit = true;
            o.Password.RequiredLength = 10;
            o.Password.RequireLowercase = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireNonAlphanumeric = true;
        })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        // No need AddAuthorization
        builder.Services.AddControllers();

        var app = builder.Build();

        await app.InitializeAdminUserAsync();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }

    private static async Task InitializeAdminUserAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var user = new IdentityUser
        {
            UserName = "kinsta",
            Email = "kinsta_@yahoo.eu",
        };

        var result = await userManager.CreateAsync(user, password: ComplicatedPassword);

        await roleManager.CreateAsync(new IdentityRole { Name = "admin" });
        await userManager.AddToRoleAsync(user, "admin");
    }
}
