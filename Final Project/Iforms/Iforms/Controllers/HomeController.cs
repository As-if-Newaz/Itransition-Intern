using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly TemplateService templateService;
        private readonly TagService tagService;

        public HomeController(TemplateService templateService, TagService tagService)
        {
            this.templateService = templateService;
            this.tagService = tagService;
        }

        public IActionResult Index(string errorMessage = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMsg"] = errorMessage;
            }
            var currentUserId = GetCurrentUserId();

            var latestTemplates = templateService.GetLatestTemplates(10, currentUserId);
            var popularTemplates = templateService.GetMostPopularTemplates(5, currentUserId);
            var tagCloud = tagService.GetTagCloud();

            var model = new HomeViewModel
            {
                LatestTemplates = latestTemplates.ToList(),
                PopularTemplates = popularTemplates.ToList(),
                TagCloud = tagCloud.ToList()
            };

            return View(model);
        }
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
