using Microsoft.EntityFrameworkCore;
using PresentationCollab.Data;
using PresentationCollab.Models;

namespace PresentationCollab.Services
{
    public class PresentatonService : IPresentationService
    {
        private readonly ApplicationDbContext db;

        public PresentatonService (ApplicationDbContext context)
        {
            this.db = context;
        }

        public List<Presentation> GetAllPresentations()
        {
            return db.Presentations
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public Presentation GetPresentationById(int id)
        {
            return db.Presentations
                .Include(p => p.Slides)
                .Include(p => p.ConnectedUsers)
                .FirstOrDefault(p => p.Id == id);
        }

        public Presentation CreatePresentation(string title, string creatorName)
        {
            var presentation = new Presentation
            {
                Title = title,
                CreatorName = creatorName,
                CreatedAt = DateTime.UtcNow,
                ConnectedUsers = new List<User>
                {
                    new User
                    {
                        Name = creatorName,
                        Role = UserRole.Creator
                    }
                }
            };

            db.Presentations.Add(presentation);
            db.SaveChanges();
            return presentation;
        }

        public Slide AddSlide(int presentationId, string content)
        {
            var presentation = db.Presentations
                .Include(p => p.Slides)
                .FirstOrDefault(p => p.Id == presentationId);

            if (presentation == null)
                throw new ArgumentException("Presentation not found");

            var slide = new Slide
            {
                PresentationId = presentationId,
                Content = content,
                Order = presentation.Slides.Count + 1,
                LastModified = DateTime.UtcNow
            };

            db.Slides.Add(slide);
            db.SaveChanges();
            return slide;
        }

        public void UpdateSlide(int slideId, string content)
        {
            var slide = db.Slides.Find(slideId);
            if (slide == null)
                throw new ArgumentException("Slide not found");

            slide.Content = content;
            slide.LastModified = DateTime.UtcNow;
            db.SaveChanges();
        }

        public User UpdateUserRole(int presentationId, string nickname, UserRole role)
        {
            var userPresence = db.UserPresences
                .FirstOrDefault(u => u.PresentationId == presentationId && u.Name == nickname);

            if (userPresence == null)
                throw new ArgumentException("User not found in presentation");

            userPresence.Role = role;
            db.SaveChanges();
            return userPresence;
        }

        public User AddUserToPresentation(int presentationId, string name)
        {
            var userPresence = new User
            {
                PresentationId = presentationId,
                Name = name,
                Role = UserRole.Viewer
            };

            db.UserPresences.Add(userPresence);
            db.SaveChanges();
            return userPresence;
        }
    }
}
