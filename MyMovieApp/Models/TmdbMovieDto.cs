using System.Text.Json.Serialization;

namespace MyMovieApp.Models
{
    public class TmdbMovieDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string PosterPath { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int> GenreIds { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        // Bunlar API'den direkt gelmiyor ama biz atayacağız
        public string GenreNames { get; set; }    // → Action, Drama
        public string Director { get; set; }      // → Yönetmen ismi
    }

    public class TmdbSearchResult
    {
        [JsonPropertyName("results")]
        public List<TmdbMovieDto> Results { get; set; }
    }
}
