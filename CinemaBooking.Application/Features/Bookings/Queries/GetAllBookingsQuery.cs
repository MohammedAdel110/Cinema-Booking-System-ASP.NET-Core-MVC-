namespace CinemaBooking.Application.Features.Bookings.Queries;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record GetAllBookingsQuery : IRequest<Result<List<BookingDto>>>;

public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, Result<List<BookingDto>>>
{
    private readonly IRepository<Booking> _repository;
    private readonly IMapper _mapper;

    public GetAllBookingsQueryHandler(IRepository<Booking> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<BookingDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<BookingDto>>(items));
    }
}
