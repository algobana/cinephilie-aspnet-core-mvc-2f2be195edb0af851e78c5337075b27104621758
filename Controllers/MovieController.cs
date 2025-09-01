using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMovieApp.Data;
using MyMovieApp.Models;
using MyMovieApp.Services;

public class MovieController : Controller
{
    private readonly TMDbClient _tmdbClient;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MovieController(TMDbClient tmdbClient, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _tmdbClient = tmdbClient;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Search(string query, int? year, int? genreId, double? minRating)
    {
        // ViewBag ile Genre listesini yolla (dropdownda kullanacağız)
        ViewBag.Genres = await _tmdbClient.GetGenresAsync();

        if (string.IsNullOrWhiteSpace(query))
        {
            return View(new List<TmdbMovieDto>());
        }

        var results = await _tmdbClient.SearchMoviesAsync(query);

        // 🎯 Filtreler
        if (year.HasValue)
        {
            results = results.Where(m => !string.IsNullOrWhiteSpace(m.ReleaseDate) &&
                                         DateTime.TryParse(m.ReleaseDate, out var d) &&
                                         d.Year == year.Value).ToList();
        }

        if (genreId.HasValue)
        {
            results = results.Where(m => m.GenreIds != null && m.GenreIds.Contains(genreId.Value)).ToList();
        }

        if (minRating.HasValue)
        {
            results = results.Where(m => m.VoteAverage >= minRating.Value).ToList();
        }

        return View(results);
    }



    public async Task<IActionResult> Details(int tmdbId)
    {
        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);

            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddToWatchlist(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        bool alreadyExists = await _context.UserMovieLists
            .AnyAsync(x => x.UserId == userId && x.MovieId == movie.MovieId);

        if (!alreadyExists)
        {
            _context.UserMovieLists.Add(new UserMovieList
            {
                UserId = userId,
                MovieId = movie.MovieId
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { tmdbId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> LikeMovie(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        bool alreadyLiked = await _context.LikedMovies
            .AnyAsync(x => x.UserId == userId && x.MovieId == movie.MovieId);

        if (!alreadyLiked)
        {
            _context.LikedMovies.Add(new LikedMovie
            {
                UserId = userId,
                MovieId = movie.MovieId
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { tmdbId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> MarkAsWatched(int tmdbId)
    {
        var userId = _userManager.GetUserId(User);

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        // Aynı kullanıcı aynı filmi daha önce izlemiş mi?
        bool alreadyWatched = await _context.WatchedMovies
            .AnyAsync(w => w.UserId == userId && w.MovieId == movie.MovieId);

        if (!alreadyWatched)
        {
            _context.WatchedMovies.Add(new WatchedMovie
            {
                UserId = userId,
                MovieId = movie.MovieId
            });

            movie.TotalWatchedCount++; // İzlenme sayısını arttır
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { tmdbId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> RateMovie(int tmdbId, int score)
    {
        if (score < 1 || score > 10)
        {
            ModelState.AddModelError("", "Puan 1 ile 10 arasında olmalıdır.");
            return RedirectToAction("Details", new { tmdbId });
        }

        var userId = _userManager.GetUserId(User);

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        // Kullanıcı daha önce puan vermiş mi?
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movie.MovieId);

        if (existingRating != null)
        {
            existingRating.Score = score; // puanı güncelle
        }
        else
        {
            _context.Ratings.Add(new Rating
            {
                UserId = userId,
                MovieId = movie.MovieId,
                Score = score
            });
        }

        // Ortalama puanı hesapla
        await _context.SaveChangesAsync();

        var ratings = await _context.Ratings
            .Where(r => r.MovieId == movie.MovieId)
            .ToListAsync();

        movie.AverageRating = ratings.Average(r => r.Score);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { tmdbId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> PostComment(int tmdbId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction("Details", new { tmdbId });
        }

        var userId = _userManager.GetUserId(User);

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
        if (movie == null)
        {
            var tmdbMovie = await _tmdbClient.GetMovieDetailsAsync(tmdbId);
            movie = new Movie
            {
                TmdbId = tmdbMovie.Id,
                Title = tmdbMovie.Title,
                Overview = tmdbMovie.Overview,
                ReleaseDate = DateTime.TryParse(tmdbMovie.ReleaseDate, out var rd) ? rd : DateTime.MinValue,
                PosterPath = tmdbMovie.PosterPath,
                ImagePath = "/images/no-image.png",
                Genre = "Unknown",
                AverageRating = 0,
                TotalWatchedCount = 0
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        var comment = new Comment
        {
            UserId = userId,
            MovieId = movie.MovieId,
            Content = content,
            CreatedAt = DateTime.Now
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { tmdbId });
    }

}
