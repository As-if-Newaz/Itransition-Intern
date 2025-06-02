// Removed the invalid namespace reference as 'Extensions' does not exist in 'Fake_Book_Generator'
using Bogus;
using Fake_Book_Generator.Models;
using System.IO;

namespace Fake_Book_Generator.Services
{
    public class BookGeneratorService
    {
        private readonly Dictionary<string, Faker<Book>> _fakers = new();
        private readonly Dictionary<string, Bogus.Faker> _baseFakers = new();
        private readonly List<string> bookTitles;

        public BookGeneratorService()
        {
            this.bookTitles = File.ReadAllLines("Data/books.txt").ToList();
            AddcountryFaker("en_US");
            AddcountryFaker("ja");
            AddcountryFaker("fr");
            AddcountryFaker("de");
        }
        private void AddcountryFaker(string country)
        {
            _baseFakers[country] = new Bogus.Faker(country);
            _fakers[country] = new Faker<Book>(country)
                .RuleFor(b => b.ISBN, f => GenerateISBN(f))
                .RuleFor(b => b.Title, f => country == "en_US" ? f.PickRandom(bookTitles) : f.Lorem.Sentence())
                .RuleFor(b => b.Authors, f => Enumerable.Range(1, f.Random.Number(1, 3))
                    .Select(_ => f.Name.FullName())
                    .ToList())
                .RuleFor(b => b.Publisher, f => $"{f.Company.CompanyName()}, {f.Date.Past(20).Year}");
        }

        private string GenerateISBN(Bogus.Faker f)
        {
            return $"978-{f.Random.Number(0, 9)}-{f.Random.Number(100, 999)}-{f.Random.Number(10000, 99999)}-{f.Random.Number(0, 9)}";
        }

        private int GenerateCountWithFraction(double average, Bogus.Faker faker)
        {
            var whole = (int)Math.Floor(average);
            var fractional = average - whole;
            return whole + (faker.Random.Double() < fractional ? 1 : 0);
        }

        private List<Review> GenerateReviews(int count, string country, Bogus.Faker faker)
        {
            return Enumerable.Range(0, count)
                .Select(_ => new Review
                {
                    Text = country == "en_US" ? faker.Rant.Review() : faker.Lorem.Paragraph(3),
                    Reviewer = faker.Person.FullName
                })
                .ToList();
        }

        public List<Book> GenerateBooks(string country, int seed, double avgLikes, double avgReviews, int startIndex, int count)
        {
            var faker = _fakers[country];
            var baseFaker = _baseFakers[country];
            faker.UseSeed(seed + startIndex);
            baseFaker.Random = new Randomizer(seed + startIndex);

            return Enumerable.Range(0, count)
                .Select(i => {
                    var book = faker.Generate();
                    book.Index = startIndex + i;
                    book.Likes = GenerateCountWithFraction(avgLikes, baseFaker);
                    book.Reviews = GenerateReviews(GenerateCountWithFraction(avgReviews, baseFaker), country, baseFaker);
                    return book;
                })
                .ToList();
        }


    }
}
