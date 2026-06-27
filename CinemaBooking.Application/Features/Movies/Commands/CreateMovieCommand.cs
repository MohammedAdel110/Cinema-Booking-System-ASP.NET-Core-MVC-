namespace CinemaBooking.Application.Features.Movies.Commands;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;

public record CreateMovieCommand(string Title, string Description, int DurationMinutes, DateTime ReleaseDate, int CategoryId) : IRequest<Result<int>>;

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Result<int>>
{
    private readonly IRepository<Movie> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMovieCommandHandler(IRepository<Movie> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            ReleaseDate = request.ReleaseDate,
            CategoryId = request.CategoryId
        };

        await _repository.AddAsync(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(movie.Id);
    }
}

public class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
