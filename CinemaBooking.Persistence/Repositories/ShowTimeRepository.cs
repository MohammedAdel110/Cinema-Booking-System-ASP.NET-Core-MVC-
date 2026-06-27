namespace CinemaBooking.Persistence.Repositories;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

public class ShowTimeRepository : Repository<ShowTime>, IShowTimeRepository
{
    public ShowTimeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ShowTime?> GetShowTimeWithDetailsAsync(int id)
    {
        return await _context.ShowTimes
            .Include(s => s.Hall)
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
