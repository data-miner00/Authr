namespace Authr.Cookie.Mvc.Controllers;

using Authr.Cookie.Mvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpPost("/mvc/login")]
    public async Task<IActionResult> Login()
    {
        // Avoid storing so much data in cookie, else it will reach max 4096Bytes and chunked to small pieces
        // This is bad lots of data consume bandwitdh
        var claimsPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                    new(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()),
                ],
            "default"));

        await HttpContext.SignInAsync("default", claimsPrincipal);
        return Ok();
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
