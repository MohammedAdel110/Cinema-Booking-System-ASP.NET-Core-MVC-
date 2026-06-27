namespace CinemaBooking.Application.Features.ShowTimes;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record CreateShowTimeCommand(int MovieId, int HallId, DateTime StartTime, decimal TicketPrice) : IRequest<Result<int>>;
public record UpdateShowTimeCommand(int Id, int MovieId, int HallId, DateTime StartTime, decimal TicketPrice) : IRequest<Result>;
public record DeleteShowTimeCommand(int Id) : IRequest<Result>;

public record GetAllShowTimesQuery : IRequest<Result<List<ShowTimeDto>>>;
public record GetShowTimeByIdQuery(int Id) : IRequest<Result<ShowTimeDto>>;

public class ShowTimeCommandHandler : 
    IRequestHandler<CreateShowTimeCommand, Result<int>>,
    IRequestHandler<UpdateShowTimeCommand, Result>,
    IRequestHandler<DeleteShowTimeCommand, Result>
{
    private readonly IRepository<ShowTime> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ShowTimeCommandHandler(IRepository<ShowTime> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateShowTimeCommand request, CancellationToken cancellationToken)
    {
        var item = new ShowTime { MovieId = request.MovieId,
            HallId = request.HallId,
            StartTime = request.StartTime,
            TicketPrice = request.TicketPrice };
        await _repository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }

    public async Task<Result> Handle(UpdateShowTimeCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("ShowTime.NotFound", "Not found."));

        item.MovieId = request.MovieId;
        item.HallId = request.HallId;
        item.StartTime = request.StartTime;
        item.TicketPrice = request.TicketPrice;
        
        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteShowTimeCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("ShowTime.NotFound", "Not found."));

        _repository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateShowTimeValidator : AbstractValidator<CreateShowTimeCommand>
{
    public CreateShowTimeValidator()
    {
        RuleFor(v => v.MovieId).GreaterThan(0);
        RuleFor(v => v.HallId).GreaterThan(0);
        RuleFor(v => v.TicketPrice).GreaterThan(0);
    }
}

public class UpdateShowTimeValidator : AbstractValidator<UpdateShowTimeCommand>
{
    public UpdateShowTimeValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.MovieId).GreaterThan(0);
        RuleFor(v => v.HallId).GreaterThan(0);
        RuleFor(v => v.TicketPrice).GreaterThan(0);
    }
}

public class ShowTimeQueriesHandler :
    IRequestHandler<GetAllShowTimesQuery, Result<List<ShowTimeDto>>>,
    IRequestHandler<GetShowTimeByIdQuery, Result<ShowTimeDto>>
{
    private readonly IRepository<ShowTime> _repository;
    private readonly IMapper _mapper;

    public ShowTimeQueriesHandler(IRepository<ShowTime> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<ShowTimeDto>>> Handle(GetAllShowTimesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<ShowTimeDto>>(items));
    }

    public async Task<Result<ShowTimeDto>> Handle(GetShowTimeByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure<ShowTimeDto>(new Error("ShowTime.NotFound", "Not found."));
        return Result.Success(_mapper.Map<ShowTimeDto>(item));
    }
}
