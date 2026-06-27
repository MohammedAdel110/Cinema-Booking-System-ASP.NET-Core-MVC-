using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
