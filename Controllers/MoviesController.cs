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

    [HttpGet]
    public override IActionResult Index()
    {
        //Henter parametre fra get requesten og binder til var
        var query = HttpContext.Request.Query["query"].ToString();
        var language = HttpContext.Request.Query["language"].ToString() ?? "en-US";
        var includeAdult = HttpContext.Request.Query["include_adult"].ToString() ?? "false";
        var year = HttpContext.Request.Query["year"].ToString();
        var region = HttpContext.Request.Query["region"].ToString();

        List<MovieSearchResult.MovieItem> results = new();


        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                //Bygger url til tmdb request
                var client = _httpClientFactory.CreateClient();
                var token = _config["TMDB:BearerToken"];

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
                //binder responsen til var
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
                        _logger.LogInformation("TMDB returned {Count} results", results.Count);
                    }
                    else
                    {
                        _logger.LogWarning("TMDB returned no results.");
                    }
                }
                else
                {
                    _logger.LogWarning("TMDB API call failed with status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling TMDB API");
            }
        }
        //Passer data til cshtml
        ViewData["Query"] = query;
        ViewData["Results"] = results;
        //returnerer 
        return CurrentTemplate(CurrentPage);
    }

}
