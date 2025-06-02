using Fake_Book_Generator.Models;
using Fake_Book_Generator.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace Fake_Book_Generator.Controllers
{
    public class HomeController : Controller
    {
        private readonly BookGeneratorService bookGenerator;

        public HomeController(BookGeneratorService bookGenerator)
        {
            this.bookGenerator = bookGenerator;
        }
       
        public IActionResult Index(string country = "en_US", int seed = 0, double avgLikes = 0, double avgReviews = 0, int page = 1, string? selectedBook = null)
        {
            ViewData["country"] = country;
            ViewData["seed"] = seed;
            ViewData["avgLikes"] = avgLikes;
            ViewData["avgReviews"] = avgReviews;
            ViewData["page"] = page;
            ViewData["currentPage"] = page;
            ViewData["selectedBook"] = selectedBook;
            var startIndex = (page - 1) * 20;
            var books = bookGenerator.GenerateBooks(country, seed, avgLikes, avgReviews, startIndex, 20);
            ViewData["books"] = books;
            return View();
        }

        public IActionResult ExportToCsv(string country = "en_US", int seed = 0, double avgLikes = 0, double avgReviews = 0, int page = 1, int totalPages = 1)
        {
            var books = new List<Book>();
            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                var startIndex = (currentPage - 1) * 20;
                var pageBooks = bookGenerator.GenerateBooks(country, seed, avgLikes, avgReviews, startIndex, 20);
                books.AddRange(pageBooks);
            }
            
            var csv = new StringBuilder();
            csv.AppendLine("Index,ISBN,Title,Authors,Publisher,Likes,Reviews");           
            foreach (var book in books)
            {
                var authors = string.Join(";", book.Authors);
                var reviews = book.Reviews.Count;
                csv.AppendLine($"{book.Index},{book.ISBN},\"{book.Title}\",\"{authors}\",\"{book.Publisher}\",{book.Likes},{reviews}");
            }
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv","books_export_" +country+ ".csv");
        }
    }
}
