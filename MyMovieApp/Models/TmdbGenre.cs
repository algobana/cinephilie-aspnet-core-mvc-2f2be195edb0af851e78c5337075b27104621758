using System.Text.Json.Serialization;

namespace MyMovieApp.Models
{
    public class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class TmdbGenreResponse
    {
        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; }
    }
}
