using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace User_Management.Auth
{
    public class Logged : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.Session.GetString("userId");
            if (string.IsNullOrEmpty(user))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
