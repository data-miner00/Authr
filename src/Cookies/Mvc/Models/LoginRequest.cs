namespace Mvc.Models;

using Microsoft.AspNetCore.Mvc;

public sealed record LoginRequest
{
    [FromForm(Name = "username")]
    public string Username { get; set; }

    [FromForm(Name = "password")]
    public string Password { get; set; }
}
