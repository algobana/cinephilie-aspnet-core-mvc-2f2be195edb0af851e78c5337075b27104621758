using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MyMovieApp.Models;

namespace MyMovieApp.Services
{
    public class TMDbClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        // Bu sadece v3 API Key ile çalışan arama için kullanılacak
        private readonly string _apiKey;
        // Bu v4 JWT token (Bearer ile gönderilen), detay çekmek için kullanılacak
        private readonly string _readAccessToken;

        public TMDbClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["TMDBSettings:ApiKey"];
            _readAccessToken = _configuration["TMDBSettings:ReadAccessToken"];
        }

        public async Task<List<TmdbMovieDto>> SearchMoviesAsync(string query)
        {
            string url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<TmdbMovieDto>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var genres = await GetGenresAsync(); // tür listesi al

            foreach (var movie in result?.Results ?? new List<TmdbMovieDto>())
            {
                var matchedGenres = genres
                    .Where(g => movie.GenreIds != null && movie.GenreIds.Contains(g.Id))
                    .Select(g => g.Name);

                movie.GenreNames = string.Join(", ", matchedGenres);
            }

            return result?.Results ?? new List<TmdbMovieDto>();
        }


        public async Task<TmdbMovieDto> GetMovieDetailsAsync(int tmdbId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.themoviedb.org/3/movie/{tmdbId}?language=en-US"),
                Headers =
        {
            { "accept", "application/json" },
            { "Authorization", $"Bearer {_readAccessToken}" }
        }
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<TmdbMovieDto>(json);

            //  Yönetmeni getir
            movie.Director = await GetMovieDirectorAsync(tmdbId);

            return movie;
        }


        public async Task<List<TmdbMovieDto>> GetPopularMoviesAsync()
        {
            string url = $"https://api.themoviedb.org/3/movie/popular?language=en-US&page=1&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<TmdbMovieDto>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Results ?? new List<TmdbMovieDto>();
        }

        public async Task<List<TmdbMovieDto>> GetNowPlayingMoviesAsync()
        {
            string url = $"https://api.themoviedb.org/3/movie/now_playing?language=en-US&page=1&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<TmdbMovieDto>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Results ?? new List<TmdbMovieDto>();
        }

        public async Task<List<TmdbMovieDto>> GetTopRatedMoviesAsync()
        {
            string url = $"https://api.themoviedb.org/3/movie/top_rated?language=en-US&page=1&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return new List<TmdbMovieDto>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbSearchResult>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Results ?? new List<TmdbMovieDto>();
        }

        private List<TmdbGenre> _genresCache = new();

        public async Task<List<TmdbGenre>> GetGenresAsync()
        {
            if (_genresCache.Any()) return _genresCache;

            string url = $"https://api.themoviedb.org/3/genre/movie/list?language=en&api_key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<TmdbGenre>();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TmdbGenreResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _genresCache = result?.Genres ?? new List<TmdbGenre>();
            return _genresCache;
        }

        public async Task<string> GetMovieDirectorAsync(int tmdbId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://api.themoviedb.org/3/movie/{tmdbId}/credits"),
                Headers =
                     {
                         { "accept", "application/json" },
                         { "Authorization", $"Bearer {_readAccessToken}" }
                     }
                                };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return "Unknown";

            var content = await response.Content.ReadAsStringAsync();

            using var jsonDoc = JsonDocument.Parse(content);
            var crew = jsonDoc.RootElement.GetProperty("crew");

            foreach (var member in crew.EnumerateArray())
            {
                if (member.TryGetProperty("job", out var jobProp) && jobProp.GetString() == "Director")
                {
                    return member.GetProperty("name").GetString() ?? "Unknown";
                }
            }

            return "Unknown";
        }




    }
}
