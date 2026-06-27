namespace CinemaBooking.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCanceled { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int ShowTimeId { get; set; }
    public ShowTime ShowTime { get; set; } = null!;

    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
}
