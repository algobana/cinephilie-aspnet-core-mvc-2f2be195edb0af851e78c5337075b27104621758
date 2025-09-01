namespace MyMovieApp.Models
{
    public class WatchedMovie
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public DateTime WatchedAt { get; set; } = DateTime.Now;
    }

}
