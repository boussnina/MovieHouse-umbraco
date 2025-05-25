using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MovieHouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieSearchController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public MovieSearchController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();
            _config = config;

        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var url = $"https://api.themoviedb.org/3/search/movie?query={query}";
            //Tager token fra appsettings
            var token = _config["TMDB:BearerToken"];
        
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            //Laver API quest med token og binder resultatet i response
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Failed to fetch movie data.");
            }

            var json = await response.Content.ReadAsStringAsync();
            //returnerer svaret
            return Content(json, "application/json");
        }
    }
}
