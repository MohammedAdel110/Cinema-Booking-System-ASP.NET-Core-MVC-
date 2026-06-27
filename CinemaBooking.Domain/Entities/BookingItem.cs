namespace CinemaBooking.Domain.Entities;

public class BookingItem
{
    public int Id { get; set; }
    public decimal Price { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
}
