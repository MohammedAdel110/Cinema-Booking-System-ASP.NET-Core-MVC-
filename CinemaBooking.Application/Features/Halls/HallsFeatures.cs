namespace CinemaBooking.Application.Features.Halls;

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

public record CreateHallCommand(string Name, int Capacity, int CinemaId) : IRequest<Result<int>>;
public record UpdateHallCommand(int Id, string Name, int Capacity, int CinemaId) : IRequest<Result>;
public record DeleteHallCommand(int Id) : IRequest<Result>;

public record GetAllHallsQuery : IRequest<Result<List<HallDto>>>;
public record GetHallByIdQuery(int Id) : IRequest<Result<HallDto>>;

public class HallCommandHandler : 
    IRequestHandler<CreateHallCommand, Result<int>>,
    IRequestHandler<UpdateHallCommand, Result>,
    IRequestHandler<DeleteHallCommand, Result>
{
    private readonly IRepository<Hall> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public HallCommandHandler(IRepository<Hall> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateHallCommand request, CancellationToken cancellationToken)
    {
        var item = new Hall { Name = request.Name,
            Capacity = request.Capacity,
            CinemaId = request.CinemaId };
        await _repository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }

    public async Task<Result> Handle(UpdateHallCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("Hall.NotFound", "Not found."));

        item.Name = request.Name;
        item.Capacity = request.Capacity;
        item.CinemaId = request.CinemaId;
        
        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteHallCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("Hall.NotFound", "Not found."));

        _repository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateHallValidator : AbstractValidator<CreateHallCommand>
{
    public CreateHallValidator()
    {
        RuleFor(v => v.Name).NotEmpty();
        RuleFor(v => v.Capacity).GreaterThan(0);
        RuleFor(v => v.CinemaId).GreaterThan(0);
    }
}

public class UpdateHallValidator : AbstractValidator<UpdateHallCommand>
{
    public UpdateHallValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty();
        RuleFor(v => v.Capacity).GreaterThan(0);
        RuleFor(v => v.CinemaId).GreaterThan(0);
    }
}

public class HallQueriesHandler :
    IRequestHandler<GetAllHallsQuery, Result<List<HallDto>>>,
    IRequestHandler<GetHallByIdQuery, Result<HallDto>>
{
    private readonly IRepository<Hall> _repository;
    private readonly IMapper _mapper;

    public HallQueriesHandler(IRepository<Hall> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<HallDto>>> Handle(GetAllHallsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<HallDto>>(items));
    }

    public async Task<Result<HallDto>> Handle(GetHallByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure<HallDto>(new Error("Hall.NotFound", "Not found."));
        return Result.Success(_mapper.Map<HallDto>(item));
    }
}
