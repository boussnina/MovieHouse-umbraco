using Microsoft.EntityFrameworkCore;
using MovieHouse.Models;

namespace MovieHouse.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<MovieRating> Ratings { get; set; }

        public DbSet<MovieFavorite> Favorites { get; set; }
    }
}
