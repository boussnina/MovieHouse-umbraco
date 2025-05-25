using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using MovieHouse.Data;
using MovieHouse.Models;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MovieHouse.Controllers
{
    public class FavoritesController : RenderController
    {
        private readonly ILogger<FavoritesController> _logger;
        private readonly MovieDbContext _db;

        public FavoritesController(
            ILogger<FavoritesController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor contextAccessor,
            MovieDbContext db)
            : base(logger, viewEngine, contextAccessor)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public override IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Attempt to view favorites without authentication.");
                return Unauthorized();
            }

            var userName = User.Identity.Name;

            var userFavorites = _db.Favorites
                .Where(f => f.UserName == userName)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            ViewData["Favorites"] = userFavorites;

            return CurrentTemplate(CurrentPage);
        }

        [HttpPost]
        public IActionResult Index(IFormCollection form)
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized favorite attempt.");
                return Unauthorized();
            }

            var userName = User.Identity.Name;

            var favorite = new MovieFavorite
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
                UserName = userName,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("❤️ Adding favorite: {Title} by {User}", favorite.Title, userName);

            _db.Favorites.Add(favorite);
            _db.SaveChanges();

            TempData["FavoriteSuccess"] = $"\"{favorite.Title}\" added to your favorites!";

            // Repopulate ViewData["Favorites"] so it shows on refresh
            var userFavorites = _db.Favorites
                .Where(f => f.UserName == userName)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            ViewData["Favorites"] = userFavorites;

            return CurrentTemplate(CurrentPage);
        }
    }
}