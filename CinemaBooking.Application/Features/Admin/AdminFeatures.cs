namespace CinemaBooking.Application.Features.Admin;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record DashboardStatisticsDto(int TotalMovies, int TotalBookings, decimal TotalRevenue, int TotalCinemas);

public record GetDashboardStatisticsQuery : IRequest<Result<DashboardStatisticsDto>>;

public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, Result<DashboardStatisticsDto>>
{
    private readonly IRepository<Movie> _movieRepo;
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IRepository<Cinema> _cinemaRepo;

    public GetDashboardStatisticsQueryHandler(
        IRepository<Movie> movieRepo, 
        IRepository<Booking> bookingRepo, 
        IRepository<Cinema> cinemaRepo)
    {
        _movieRepo = movieRepo;
        _bookingRepo = bookingRepo;
        _cinemaRepo = cinemaRepo;
    }

    public async Task<Result<DashboardStatisticsDto>> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepo.GetAllAsync();
        var bookings = await _bookingRepo.GetAllAsync();
        var cinemas = await _cinemaRepo.GetAllAsync();

        var totalMovies = movies.Count;
        var totalBookings = bookings.Count;
        var totalRevenue = bookings.Where(b => !b.IsCanceled).Sum(b => b.TotalAmount);
        var totalCinemas = cinemas.Count;

        return Result.Success(new DashboardStatisticsDto(totalMovies, totalBookings, totalRevenue, totalCinemas));
    }
}
