using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace MyMovieApp.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        public string Title { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Genre { get; set; }

        public string ImagePath { get; set; } // manuel yüklenen görselin yolu

        public string Overview { get; set; } // TMDb'den açıklama

        public string PosterPath { get; set; } // TMDb poster yolu

        public double AverageRating { get; set; }

        public int TotalWatchedCount { get; set; }

        public int TmdbId { get; set; }

        public ICollection<Rating> Ratings { get; set; }

        public ICollection<Comment> Comments { get; set; }

        [NotMapped] // bu satır veritabanına eklenmesini engeller
        public string? Director { get; set; }
    }
}
