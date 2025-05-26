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
        //Henter alle favorites forbundet med den bruger der er logget ind
        {
            if (!User.Identity.IsAuthenticated)
            {   //Hvis bruger ikke er logget ind binder den ikke data
                return CurrentTemplate(CurrentPage);
            }
            
            var userName = User.Identity.Name;

            var userFavorites = _db.Favorites
                .Where(f => f.UserName == userName)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();
            //Binder brugerens favoritter så de kan vises i cshtml
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
            //Opretter en ny favorite, der kan gemmes i db og smider data på den
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
            //Gemmes i favorit tabellen i db
            _db.Favorites.Add(favorite);
            _db.SaveChanges();
            //Gemmer en besked til brugeren
            TempData["FavoriteSuccess"] = $"\"{favorite.Title}\" added to your favorites!";

            //Henter den nye favorit liste
            var userFavorites = _db.Favorites
                .Where(f => f.UserName == userName)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();
            //Gemmer listen så den kan vises igen
            ViewData["Favorites"] = userFavorites;

            return CurrentTemplate(CurrentPage);
        }
    }
}