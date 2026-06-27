namespace CinemaBooking.Application.DTOs;

public class ShowTimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string? MovieTitle { get; set; }
    public int HallId { get; set; }
    public string? HallName { get; set; }
    public DateTime StartTime { get; set; }
    public decimal TicketPrice { get; set; }
}
