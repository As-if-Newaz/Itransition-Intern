using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Iforms.MVC.Controllers
{
    [AuthenticatedAdminorUser]
    public class ApiTokenController : Controller
    {
        private readonly ApiTokenService apiTokenService;
        public ApiTokenController(ApiTokenService apiTokenService)
        {
            this.apiTokenService = apiTokenService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var tokens = apiTokenService.GetByUserId(userId);
            return View(tokens);
        }

        [HttpPost]
        public IActionResult Create(string? name = null)
        {
            var userId = GetCurrentUserId();
            var token = apiTokenService.Create(userId, name);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Revoke(int id)
        {
            var userId = GetCurrentUserId();
            var token = apiTokenService.GetById(id);
            if (token == null || token.UserId != userId)
                return Forbid();
            apiTokenService.Revoke(id);
            return RedirectToAction("Index");
        }
    }
} 