namespace CinemaBooking.Domain.Entities;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int? PosterId { get; set; }
    public Poster? Poster { get; set; }

    public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    public ICollection<FavoriteMovie> FavoriteMovies { get; set; } = new List<FavoriteMovie>();
}
