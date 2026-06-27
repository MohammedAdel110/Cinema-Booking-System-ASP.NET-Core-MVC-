namespace CinemaBooking.Application.Features.Bookings.Commands;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public record CancelBookingCommand(int Id, string UserId) : IRequest<Result>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IRepository<Booking> bookingRepo, IUnitOfWork unitOfWork)
    {
        _bookingRepo = bookingRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepo.GetByIdAsync(request.Id);
        if (booking == null) return Result.Failure(new Error("Booking.NotFound", "Booking not found."));

        if (booking.UserId != request.UserId)
            return Result.Failure(new Error("Booking.Unauthorized", "You are not authorized to cancel this booking."));

        if (booking.IsCanceled)
            return Result.Failure(new Error("Booking.AlreadyCanceled", "Booking is already canceled."));

        booking.IsCanceled = true;
        
        _bookingRepo.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
