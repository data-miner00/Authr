using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<IdentityDbContext>();

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

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
using var scope = app.Services.CreateScope();

var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var usrMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

var usr = new IdentityUser
{
    UserName = "Test",
    Email = "Test",
};

await usrMgr.CreateAsync(usr, password: "test");
await roleMgr.CreateAsync(new IdentityRole { Name = "admin" });
await usrMgr.AddToRoleAsync(usr, "admin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
