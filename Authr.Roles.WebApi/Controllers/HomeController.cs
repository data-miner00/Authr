using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authr.Roles.WebApi.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("/")]
        public string Index() => "index";

        [HttpGet("/secret")]
        [Authorize(Roles = "admin")]
        public string Secret() => "secret";
    }
}
