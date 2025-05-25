using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieHouse.Models
{
    public class MovieSearchResult
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<MovieItem> Results { get; set; }

        public class MovieItem
        {
            [JsonPropertyName("adult")]
            public bool Adult { get; set; }

            [JsonPropertyName("backdrop_path")]
            public string BackdropPath { get; set; }

            [JsonPropertyName("genre_ids")]
            public List<int> GenreIds { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("original_language")]
            public string OriginalLanguage { get; set; }

            [JsonPropertyName("original_title")]
            public string OriginalTitle { get; set; }

            [JsonPropertyName("overview")]
            public string Overview { get; set; }

            [JsonPropertyName("popularity")]
            public double Popularity { get; set; }

            [JsonPropertyName("poster_path")]
            public string PosterPath { get; set; }

            [JsonPropertyName("release_date")]
            public string ReleaseDate { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }
    }
}
