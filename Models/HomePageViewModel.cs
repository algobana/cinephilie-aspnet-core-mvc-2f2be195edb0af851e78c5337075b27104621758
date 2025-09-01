namespace MyMovieApp.Models
{
    public class HomePageViewModel
    {
        public List<TmdbMovieDto> PopularMovies { get; set; } = new();
        public List<TmdbMovieDto> NowPlayingMovies { get; set; } = new();
        public List<TmdbMovieDto> TopRatedMovies { get; set; } = new();

    }
}
