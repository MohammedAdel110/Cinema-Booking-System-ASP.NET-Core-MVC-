namespace CinemaBooking.Application.Features.Cinemas;

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

public record CreateCinemaCommand(string Name, string Location) : IRequest<Result<int>>;
public record UpdateCinemaCommand(int Id, string Name, string Location) : IRequest<Result>;
public record DeleteCinemaCommand(int Id) : IRequest<Result>;

public record GetAllCinemasQuery : IRequest<Result<List<CinemaDto>>>;
public record GetCinemaByIdQuery(int Id) : IRequest<Result<CinemaDto>>;

public class CinemaCommandHandler : 
    IRequestHandler<CreateCinemaCommand, Result<int>>,
    IRequestHandler<UpdateCinemaCommand, Result>,
    IRequestHandler<DeleteCinemaCommand, Result>
{
    private readonly IRepository<Cinema> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CinemaCommandHandler(IRepository<Cinema> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateCinemaCommand request, CancellationToken cancellationToken)
    {
        var item = new Cinema { Name = request.Name,
            Location = request.Location };
        await _repository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }

    public async Task<Result> Handle(UpdateCinemaCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("Cinema.NotFound", "Not found."));

        item.Name = request.Name;
        item.Location = request.Location;
        
        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteCinemaCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("Cinema.NotFound", "Not found."));

        _repository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class CreateCinemaValidator : AbstractValidator<CreateCinemaCommand>
{
    public CreateCinemaValidator()
    {
        RuleFor(v => v.Name).NotEmpty();
        RuleFor(v => v.Location).NotEmpty();
    }
}

public class UpdateCinemaValidator : AbstractValidator<UpdateCinemaCommand>
{
    public UpdateCinemaValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty();
        RuleFor(v => v.Location).NotEmpty();
    }
}

public class CinemaQueriesHandler :
    IRequestHandler<GetAllCinemasQuery, Result<List<CinemaDto>>>,
    IRequestHandler<GetCinemaByIdQuery, Result<CinemaDto>>
{
    private readonly IRepository<Cinema> _repository;
    private readonly IMapper _mapper;

    public CinemaQueriesHandler(IRepository<Cinema> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<CinemaDto>>> Handle(GetAllCinemasQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<CinemaDto>>(items));
    }

    public async Task<Result<CinemaDto>> Handle(GetCinemaByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure<CinemaDto>(new Error("Cinema.NotFound", "Not found."));
        return Result.Success(_mapper.Map<CinemaDto>(item));
    }
}
