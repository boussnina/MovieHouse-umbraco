using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MovieHouse.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.Generic;
using MovieHouse.Data;

public class MoviesController : RenderController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MoviesController> _logger;
    private readonly IConfiguration _config;
    private readonly MovieDbContext _db;

    public MoviesController(
        ILogger<MoviesController> logger,
        ICompositeViewEngine viewEngine,
        IUmbracoContextAccessor contextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        MovieDbContext db)
        : base(logger, viewEngine, contextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = config;
        _db = db;
    }


    public override IActionResult Index()
    {
        var query = HttpContext.Request.Query["query"].ToString();
        var language = HttpContext.Request.Query["language"].ToString() ?? "en-US";
        var includeAdult = HttpContext.Request.Query["include_adult"].ToString() ?? "false";
        var year = HttpContext.Request.Query["year"].ToString();
        var region = HttpContext.Request.Query["region"].ToString();

        List<MovieSearchResult.MovieItem> results = new();

        _logger.LogInformation("🎬 MoviesController.Index() called with query: {Query}", query);

        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var token = _config["TMDB:BearerToken"];

                // Build query URL
                var urlBuilder = new StringBuilder("https://api.themoviedb.org/3/search/movie?");
                urlBuilder.Append($"query={Uri.EscapeDataString(query)}");
                urlBuilder.Append($"&language={language}");
                urlBuilder.Append($"&include_adult={includeAdult}");

                if (!string.IsNullOrWhiteSpace(year))
                    urlBuilder.Append($"&year={Uri.EscapeDataString(year)}");

                if (!string.IsNullOrWhiteSpace(region))
                    urlBuilder.Append($"&region={Uri.EscapeDataString(region)}");

                var apiUrl = urlBuilder.ToString();

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = client.Send(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;

                    var result = JsonSerializer.Deserialize<MovieSearchResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Results != null)
                    {
                        results = result.Results;
                        _logger.LogInformation("✅ TMDB returned {Count} results", results.Count);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ TMDB returned no results.");
                    }
                }
                else
                {
                    _logger.LogWarning("❌ TMDB API call failed with status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while calling TMDB API");
            }
        }

        ViewData["Query"] = query;
        ViewData["Results"] = results;

        return CurrentTemplate(CurrentPage);
    }

    [HttpPost]
    public IActionResult Index(IFormCollection form)
    {
        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("Unauthorized rating attempt.");
            return Unauthorized(); // or Redirect("/registerlogin");
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

        _logger.LogInformation("⭐ Saving rating {Rating} for movie: {Title} by {User}", movie.Rating, movie.Title, userName);

        _db.Ratings.Add(movie);
        _db.SaveChanges();
        TempData["RatingSuccess"] = $"Thanks for rating \"{form["title"]}\"!";

        ViewData["Query"] = form["query"];
        ViewData["Results"] = form["query"];

        return CurrentTemplate(CurrentPage);
    }
    
}
