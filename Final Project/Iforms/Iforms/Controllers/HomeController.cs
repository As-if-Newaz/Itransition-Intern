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

        //[HttpGet]
        //public IActionResult Search(string q, string topic, string tags, int page = 1)
        //{
        //    var searchDto = new TemplateSearchDTO
        //    {
        //        SearchTerm = q,
        //        Page = page,
        //        PageSize = 10
        //    };

        //    if (topic != null)
        //    {
        //        searchDto.Topic = new TopicDTO();
        //    }

        //    if (!string.IsNullOrEmpty(tags))
        //    {
        //        searchDto.Tags = tags.Split(',').Select(t => t.Trim()).ToList();
        //    }

        //    var currentUserId = GetCurrentUserId();
        //    var results = templateService.Search(searchDto, currentUserId);

        //    ViewBag.SearchTerm = q;
        //    ViewBag.Topic = topic;
        //    ViewBag.Tags = tags;

        //    return View(results);
        //}

        //[HttpGet]
        //public IActionResult SearchTags(string term)
        //{
        //    var tags = tagService.SearchTags(term ?? "");
        //    return Json(tags.Select(t => new { id = t.Id, name = t.Name }));
        //}

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
