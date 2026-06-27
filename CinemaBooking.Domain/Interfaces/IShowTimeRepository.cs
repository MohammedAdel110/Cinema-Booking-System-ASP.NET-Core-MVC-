namespace CinemaBooking.Domain.Interfaces;

using CinemaBooking.Domain.Entities;

public interface IShowTimeRepository : IRepository<ShowTime>
{
    Task<ShowTime?> GetShowTimeWithDetailsAsync(int id);
}
