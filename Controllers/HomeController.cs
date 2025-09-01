using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyMovieApp.Models;
using MyMovieApp.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MyMovieApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TMDbClient _tmdbClient;

        public HomeController(ILogger<HomeController> logger, TMDbClient tmdbClient)
        {
            _logger = logger;
            _tmdbClient = tmdbClient;
        }

        public async Task<IActionResult> Index()
        {
            var popularMovies = await _tmdbClient.GetPopularMoviesAsync();
            var nowPlayingMovies = await _tmdbClient.GetNowPlayingMoviesAsync();
            var topRatedMovies = await _tmdbClient.GetTopRatedMoviesAsync();

            var viewModel = new HomePageViewModel
            {
                PopularMovies = popularMovies,
                NowPlayingMovies = nowPlayingMovies,
                TopRatedMovies = topRatedMovies
            };

            return View(viewModel); // Artýk model: HomePageViewModel
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AboutUS()
        {
            return View();
        }
    }
}
