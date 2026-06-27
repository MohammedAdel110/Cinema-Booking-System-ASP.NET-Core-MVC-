namespace CinemaBooking.Application.Features.Movies.Queries;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;

public record GetAllMoviesQuery : IRequest<Result<List<MovieDto>>>;

public class GetAllMoviesQueryHandler : IRequestHandler<GetAllMoviesQuery, Result<List<MovieDto>>>
{
    private readonly IRepository<CinemaBooking.Domain.Entities.Movie> _repository;

    public GetAllMoviesQueryHandler(IRepository<CinemaBooking.Domain.Entities.Movie> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<MovieDto>>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = await _repository.GetAllAsync();
        
        var dtos = movies.Select(m => new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            DurationMinutes = m.DurationMinutes,
            ReleaseDate = m.ReleaseDate,
            CategoryId = m.CategoryId
        }).ToList();

        return Result.Success(dtos);
    }
}
