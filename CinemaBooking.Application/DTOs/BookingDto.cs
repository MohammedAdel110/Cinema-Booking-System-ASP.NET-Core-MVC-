namespace CinemaBooking.Application.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ShowTimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public DateTime ShowTimeStart { get; set; }
    public DateTime BookingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCanceled { get; set; }
    public int SeatCount { get; set; }
}
