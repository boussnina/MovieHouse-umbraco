using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using MovieHouse.Data;
using MovieHouse.Models;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MovieHouse.Controllers
{
    public class RatingsController : RenderController
    {
        private readonly MovieDbContext _db;
        private readonly ILogger _logger;

        public RatingsController(
            ILogger<RatingsController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            MovieDbContext db)
            : base(logger, viewEngine, umbracoContextAccessor)
        {
            _db = db;
            _logger = logger;
        }
        [HttpGet]
        public override IActionResult Index()
        {
            List<MovieRating> allRatings = _db.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ViewData["Ratings"] = allRatings;

            return CurrentTemplate(CurrentPage);
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized rating attempt.");
                return Unauthorized(); 
            }

            var rating = int.TryParse(form["rating"], out var r) ? r : 0;
            var userName = User.Identity.Name;

            var movie = new MovieRating
            {
                MovieId = int.Parse(form["id"]),
                Title = form["title"],
                OriginalTitle = form["originalTitle"],
                ReleaseDate = form["releaseDate"],
                PosterPath = form["posterPath"],
                BackdropPath = form["backdropPath"],
                OriginalLanguage = form["originalLanguage"],
                Overview = form["overview"],
                Popularity = double.TryParse(form["popularity"], out var popularity) ? popularity : 0,
                Adult = bool.TryParse(form["adult"], out var isAdult) && isAdult,
                GenreIds = form["genreIds"],
                Rating = rating,
                UserName = userName,
                CreatedAt = DateTime.UtcNow
            };

            _db.Ratings.Add(movie);
            _db.SaveChanges();
            TempData["RatingSuccess"] = $"Thanks for rating \"{form["title"]}\"!";

            var allRatings = _db.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ViewData["Ratings"] = allRatings;

            return CurrentTemplate(CurrentPage);
        }
    }
}
