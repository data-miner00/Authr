using Microsoft.AspNetCore.DataProtection;

namespace Authr.WebApi
{
    public class AuthService
    {
        private readonly IDataProtectionProvider idp;
        private readonly IHttpContextAccessor accessor;

        public AuthService(IDataProtectionProvider idp, IHttpContextAccessor accessor)
        {
            this.idp = idp;
            this.accessor = accessor;
        }

        public void SignIn()
        {
            var protector = this.idp.CreateProtector("auth-cookie");
            this.accessor.HttpContext.Response.Headers["set-cookie"] = $"auth={protector.Protect("usr:sharon")}";
        }
    }
}
