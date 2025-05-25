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

        public RatingsController(
            ILogger<RatingsController> logger,
            ICompositeViewEngine viewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            MovieDbContext db)
            : base(logger, viewEngine, umbracoContextAccessor)
        {
            _db = db;
        }

        public override IActionResult Index()
        {
            List<MovieRating> allRatings = _db.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            ViewData["Ratings"] = allRatings;

            return CurrentTemplate(CurrentPage);
        }
    }
}
