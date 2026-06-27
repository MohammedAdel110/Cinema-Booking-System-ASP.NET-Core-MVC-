namespace CinemaBooking.Domain.Entities;

public class Poster
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadDate { get; set; }

    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
}
