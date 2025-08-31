using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMovieApp.Models;
using System.Collections.Generic;

namespace MyMovieApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>

    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<WatchedMovie> WatchedMovies { get; set; }
        public DbSet<UserMovieList> UserMovieLists { get; set; }
        public DbSet<LikedMovie> LikedMovies { get; set; }


    }
}
