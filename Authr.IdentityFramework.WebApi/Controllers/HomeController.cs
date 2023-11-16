using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authr.IdentityFramework.WebApi.Controllers
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