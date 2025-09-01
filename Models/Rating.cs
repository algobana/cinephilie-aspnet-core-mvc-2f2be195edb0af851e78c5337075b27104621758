namespace MyMovieApp.Models
{
    public class Rating
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public int Score { get; set; } // 1 ile 10 arası
    }
}
