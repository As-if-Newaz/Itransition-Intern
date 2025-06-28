using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Iforms.MVC.Authentication
{
    public class AuthenticatedAdminorUser : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Request.Cookies["Role"];
            if (role != "Admin" && role != "User")
            {
                // Not authorized
                context.Result = new RedirectToActionResult("Login", "Auth", null);
            }
        }
    }
}
