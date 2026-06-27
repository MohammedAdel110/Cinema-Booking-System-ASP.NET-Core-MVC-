namespace CinemaBooking.Application.Features.Bookings.Commands;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record CreateBookingCommand(string UserId, int ShowTimeId, int NumberOfSeats) : IRequest<Result<int>>;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<int>>
{
    private readonly IShowTimeRepository _showTimeRepo;
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IShowTimeRepository showTimeRepo,
        IRepository<Booking> bookingRepo,
        IUnitOfWork unitOfWork)
    {
        _showTimeRepo = showTimeRepo;
        _bookingRepo = bookingRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var showTime = await _showTimeRepo.GetShowTimeWithDetailsAsync(request.ShowTimeId);
        if (showTime == null) 
            return Result.Failure<int>(new Error("Booking.NotFound", "Showtime not found."));

        if (showTime.StartTime <= DateTime.UtcNow)
        {
            return Result.Failure<int>(new Error("Booking.Started", "Cannot book a showtime that has already started."));
        }

        var currentlyBookedSeats = showTime.Bookings
            .Where(b => !b.IsCanceled)
            .SelectMany(b => b.Items)
            .Count();

        var availableSeats = showTime.Hall.Capacity - currentlyBookedSeats;

        if (request.NumberOfSeats > availableSeats)
        {
            return Result.Failure<int>(new Error("Booking.Capacity", $"Only {availableSeats} seats available."));
        }

        var booking = new Booking
        {
            UserId = request.UserId,
            ShowTimeId = request.ShowTimeId,
            BookingDate = DateTime.UtcNow,
            TotalAmount = request.NumberOfSeats * showTime.TicketPrice,
            IsCanceled = false
        };

        for (int i = 0; i < request.NumberOfSeats; i++)
        {
            booking.Items.Add(new BookingItem
            {
                Price = showTime.TicketPrice
            });
        }

        await _bookingRepo.AddAsync(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(booking.Id);
    }
}

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ShowTimeId).GreaterThan(0);
        RuleFor(x => x.NumberOfSeats).GreaterThan(0).LessThanOrEqualTo(10);
    }
}
