using System;

namespace MovieHouse.Models
{
    public class MovieFavorite
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string ReleaseDate { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public string OriginalLanguage { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public bool Adult { get; set; }
        public string GenreIds { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
    }
}
