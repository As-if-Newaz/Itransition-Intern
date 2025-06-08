using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation_Collab.Models;
using System.Diagnostics;
using System.Text;

namespace Presentation_Collab.Controllers
{
    public class PresentationController : Controller
    {
        private readonly ApplicationDBContext db;
        public PresentationController(ApplicationDBContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            var nickname = HttpContext.Session.GetString("UserNickname");
            if (string.IsNullOrEmpty(nickname))
            {
                return RedirectToAction("Index", "Home");
            }

            var presentations = db.Presentation
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            return View(presentations);
        }

        public IActionResult Create()
        {
            var nickname = HttpContext.Session.GetString("UserNickname");
            if (string.IsNullOrEmpty(nickname))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([FromForm] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["error"] = "Presentation title is required";
                return View();
            }

            var nickname = HttpContext.Session.GetString("UserNickname");
            if (string.IsNullOrEmpty(nickname))
            {
                return RedirectToAction("Index", "Home");
            }

            var presentation = new Presentation
            {
                Title = title,
                CreatorName = nickname,
                CreatedAt = DateTime.UtcNow,
                ConnectedUsers = new List<User>
                {
                    new User
                    {
                        Name = nickname,
                        Role = UserRole.Creator
                    }
                }
            };

            db.Presentation.Add(presentation);
            db.SaveChanges();

            // Create initial slide
            var initialSlide = new Slide
            {
                Order = 1,
                PresentationId = presentation.Id,
                LastModified = DateTime.UtcNow
            };
            db.Slides.Add(initialSlide);
            db.SaveChanges();

            TempData["success"] = "Presentation created successfully";
            return RedirectToAction("Edit", new { id = presentation.Id });
        }

        public IActionResult Edit(int id)
        {
            var presentation = db.Presentation
                .Include(p => p.Slides)
                .Include(p => p.ConnectedUsers)
                .FirstOrDefault(p => p.Id == id);

            if (presentation == null)
            {
                return NotFound();
            }

            var nickname = HttpContext.Session.GetString("UserNickname");
            var userExists = presentation.ConnectedUsers.Any(u => u.Name == nickname);

            if (!userExists)
            {
                var userPresence = new User
                {
                    PresentationId = id,
                    Name = nickname,
                    Role = UserRole.Viewer
                };

                db.Users.Add(userPresence);
                db.SaveChanges();
                presentation = db.Presentation
                    .Include(p => p.Slides)
                    .Include(p => p.ConnectedUsers)
                    .FirstOrDefault(p => p.Id == id);
            }

            return View(presentation);
        }

        [HttpPost]
        public async Task<JsonResult> AddSlide([FromBody] AddSlideRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            var presentation = await db.Presentation
                .Include(p => p.Slides)
                .FirstOrDefaultAsync(p => p.Id == request.PresentationId);

            if (presentation == null)
            {
                return Json(new { success = false, message = "Presentation not found" });
            }

            var nickname = HttpContext.Session.GetString("UserNickname");
            if (presentation.CreatorName != nickname)
            {
                return Json(new { success = false, message = "Only creator can add slides" });
            }

            var newOrder = presentation.Slides.Count + 1;
            var newSlide = new Slide
            {
                Order = newOrder,
                PresentationId = request.PresentationId,
                LastModified = DateTime.UtcNow,
                SvgData = request.IsBlank ? System.Text.Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"800\" height=\"600\" viewBox=\"0 0 800 600\"></svg>") : null
            };

            db.Slides.Add(newSlide);
            await db.SaveChangesAsync();

            return Json(new { success = true, slideId = newSlide.Id, order = newOrder });
        }

        public class AddSlideRequest
        {
            public int PresentationId { get; set; }
            public bool IsBlank { get; set; }
        }

        public class DeleteSlideRequest
        {
            public int SlideId { get; set; }
            public int PresentationId { get; set; }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteSlide([FromBody] DeleteSlideRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Invalid request" });
            }

            var presentation = await db.Presentation
                .Include(p => p.Slides)
                .FirstOrDefaultAsync(p => p.Id == request.PresentationId);

            if (presentation == null)
            {
                return Json(new { success = false, message = "Presentation not found" });
            }

            var nickname = HttpContext.Session.GetString("UserNickname");
            if (presentation.CreatorName != nickname)
            {
                return Json(new { success = false, message = "Only creator can delete slides" });
            }

            var slide = presentation.Slides.FirstOrDefault(s => s.Id == request.SlideId);
            if (slide == null)
            {
                return Json(new { success = false, message = "Slide not found" });
            }

            db.Slides.Remove(slide);
            var remainingSlides = presentation.Slides
                .Where(s => s.Id != request.SlideId)
                .OrderBy(s => s.Order)
                .ToList();

            for (int i = 0; i < remainingSlides.Count; i++)
            {
                remainingSlides[i].Order = i + 1;
            }

            await db.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Json(new { success = false, message = "Invalid request: Request is null" });
                }

                if (request.PresentationId <= 0)
                {
                    return Json(new { success = false, message = "Invalid request: Invalid presentation ID" });
                }

                if (string.IsNullOrEmpty(request.Username))
                {
                    return Json(new { success = false, message = "Invalid request: Username is required" });
                }

                var presentation = await db.Presentation
                    .Include(p => p.ConnectedUsers)
                    .FirstOrDefaultAsync(p => p.Id == request.PresentationId);

                if (presentation == null)
                {
                    return Json(new { success = false, message = "Presentation not found" });
                }

                var nickname = HttpContext.Session.GetString("UserNickname");
                if (presentation.CreatorName != nickname)
                {
                    return Json(new { success = false, message = "Only creator can change roles" });
                }

                var user = presentation.ConnectedUsers.FirstOrDefault(u => u.Name == request.Username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (Enum.TryParse<UserRole>(request.NewRole.ToString(), out UserRole role))
                {
                    user.Role = role;
                    await db.SaveChangesAsync();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid role value" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating role: {ex.Message}" });
            }
        }

        public class UpdateUserRoleRequest
        {
            public int PresentationId { get; set; }
            public string Username { get; set; }
            public string NewRole { get; set; } 
        }

        [HttpGet]
        [Route("get-svg/{slideId}")]
        public async Task<FileStreamResult> GetSvg(int slideId)
        {
            var slide = await db.Slides
                .Where(s => s.Id == slideId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (slide == null || slide.SvgData == null)
            {
                return new FileStreamResult(new MemoryStream(), "image/svg+xml");
            }

            var stream = new MemoryStream(slide.SvgData);
            return new FileStreamResult(stream, "image/svg+xml");
        }

        [HttpGet]
        public IActionResult Exit()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<JsonResult> DeletePresentation(int id)
        {
            try
            {
                var presentation = await db.Presentation
                    .Include(p => p.Slides)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (presentation == null)
                {
                    return Json(new { success = false, message = "Presentation not found" });
                }

                var nickname = HttpContext.Session.GetString("UserNickname");
                if (presentation.CreatorName != nickname)
                {
                    return Json(new { success = false, message = "Only the creator can delete this presentation" });
                }

                db.Slides.RemoveRange(presentation.Slides);

                db.Presentation.Remove(presentation);
                await db.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting presentation: {ex.Message}" });
            }
        }
    }
}
