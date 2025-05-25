using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MovieHouse.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Models.PublishedContent;
using System.Collections.Generic;

public class MoviesController : RenderController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MoviesController> _logger;
    private readonly IConfiguration _config;

    public MoviesController(
        ILogger<MoviesController> logger,
        ICompositeViewEngine viewEngine,
        IUmbracoContextAccessor contextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
        : base(logger, viewEngine, contextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _config = config;
    }

    public override IActionResult Index()
    {
        var query = HttpContext.Request.Query["query"].ToString();
        List<MovieSearchResult.MovieItem> results = new();

        _logger.LogInformation("🎬 MoviesController.Index() called with query: {Query}", query);

        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var token = _config["TMDB:BearerToken"];
                var apiUrl = $"https://api.themoviedb.org/3/search/movie?query={Uri.EscapeDataString(query)}";

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
                        _logger.LogWarning("⚠️ TMDB returned no results for query: {Query}", query);
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
}
