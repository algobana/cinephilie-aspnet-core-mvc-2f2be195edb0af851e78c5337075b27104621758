using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMovieApp.Data;
using MyMovieApp.Models;
using Microsoft.AspNetCore.Identity;

namespace MyMovieApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyLists()
        {
            var userId = _userManager.GetUserId(User);

            var watchlist = await _context.UserMovieLists
                .Include(x => x.Movie)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var liked = await _context.LikedMovies
                .Include(x => x.Movie)
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var viewModel = new MyListsViewModel
            {
                Watchlist = watchlist,
                Liked = liked
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWatchlist(int movieId)
        {
            var userId = _userManager.GetUserId(User);

            var entry = await _context.UserMovieLists
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId);

            if (entry != null)
            {
                _context.UserMovieLists.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyLists");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromLiked(int movieId)
        {
            var userId = _userManager.GetUserId(User);

            var entry = await _context.LikedMovies
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MovieId == movieId);

            if (entry != null)
            {
                _context.LikedMovies.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyLists");
        }
    }
}
