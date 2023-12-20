var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddCookie("cookie");

builder.Services
    .AddAuthorization()
    .AddControllers();

var app = builder.Build();

app
    .UseHttpsRedirection()
    .UseAuthentication()
    .UseAuthorization();

app.MapControllers();

app.Run();
