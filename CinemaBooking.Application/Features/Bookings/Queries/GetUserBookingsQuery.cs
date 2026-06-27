namespace CinemaBooking.Application.Features.Bookings.Queries;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record GetUserBookingsQuery(string UserId) : IRequest<Result<List<BookingDto>>>;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, Result<List<BookingDto>>>
{
    private readonly IRepository<Booking> _repository;
    private readonly IMapper _mapper;

    public GetUserBookingsQueryHandler(IRepository<Booking> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<BookingDto>>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        var userItems = items.Where(b => b.UserId == request.UserId).ToList();
        return Result.Success(_mapper.Map<List<BookingDto>>(userItems));
    }
}

public record ShowTimeBookingDetailsDto(int Id, string MovieTitle, string CinemaName, string HallName, DateTime StartTime, decimal TicketPrice, int AvailableSeats);

public record GetShowTimeDetailsForBookingQuery(int ShowTimeId) : IRequest<Result<ShowTimeBookingDetailsDto>>;

public class GetShowTimeDetailsForBookingQueryHandler : IRequestHandler<GetShowTimeDetailsForBookingQuery, Result<ShowTimeBookingDetailsDto>>
{
    private readonly IShowTimeRepository _showTimeRepo;

    public GetShowTimeDetailsForBookingQueryHandler(IShowTimeRepository showTimeRepo)
    {
        _showTimeRepo = showTimeRepo;
    }

    public async Task<Result<ShowTimeBookingDetailsDto>> Handle(GetShowTimeDetailsForBookingQuery request, CancellationToken cancellationToken)
    {
        var showTime = await _showTimeRepo.GetShowTimeWithDetailsAsync(request.ShowTimeId);
        if (showTime == null) return Result.Failure<ShowTimeBookingDetailsDto>(new Error("ShowTime.NotFound", "Showtime not found."));

        int booked = showTime.Bookings.Where(b => !b.IsCanceled).SelectMany(b => b.Items).Count();
        int available = showTime.Hall.Capacity - booked;

        var dto = new ShowTimeBookingDetailsDto(
            showTime.Id,
            showTime.Movie.Title,
            showTime.Hall.Cinema.Name,
            showTime.Hall.Name,
            showTime.StartTime,
            showTime.TicketPrice,
            available
        );

        return Result.Success(dto);
    }
}
