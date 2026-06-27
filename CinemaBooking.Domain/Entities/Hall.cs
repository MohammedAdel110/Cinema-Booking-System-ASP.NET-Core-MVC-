namespace CinemaBooking.Domain.Entities;

public class Hall
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;

    public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}
