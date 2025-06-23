using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.MVC.Authentication
{
    public class AuthenticatedUser : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var token = context.HttpContext.Request.Cookies["JWTToken"];

            if (string.IsNullOrEmpty(token))
            {
                RedirectToLogin(context, "Please log in to continue.");
                return;
            }
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    RedirectToLogin(context, "Your session has expired. Please log in again.");
                    return;
                }

                var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    RedirectToLogin(context, "Invalid session, Please log in again.");
                    return;
                }


                var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();

                var user = userService.GetById(int.Parse(userId));
                if (user == null || user.UserStatus.Equals(UserStatus.Blocked) || user.UserRole != UserRole.User)
                {
                    RedirectToLogin(context, "Invalid Session, Please log in again");
                    return;
                }
            }
            catch
            {
                RedirectToLogin(context, "An error occurred. Please log in again.");
            }
        }
        private void RedirectToLogin(AuthorizationFilterContext context, string message)
        {
            context.HttpContext.Response.Cookies.Delete("JWTToken");
            var redirectResult = new RedirectToActionResult("Login", "Auth", new { errorMessage = message });
            context.Result = redirectResult;
        }
    }
}
