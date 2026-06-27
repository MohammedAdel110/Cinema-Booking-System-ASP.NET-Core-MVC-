namespace CinemaBooking.Application.Features.Movies;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record CreateMovieCommand(string Title, string Description, int DurationMinutes, DateTime ReleaseDate, int CategoryId, IFormFile? PosterFile) : IRequest<Result<int>>;
public record UpdateMovieCommand(int Id, string Title, string Description, int DurationMinutes, DateTime ReleaseDate, int CategoryId, IFormFile? PosterFile) : IRequest<Result>;
public record DeleteMovieCommand(int Id) : IRequest<Result>;

public record GetMovieByIdQuery(int Id) : IRequest<Result<MovieDto>>;
public record GetAllMoviesQuery(int? CategoryId = null, string? SearchTerm = null) : IRequest<Result<List<MovieDto>>>;

public record MovieDetailsDto(int Id, string Title, string Description, int DurationMinutes, DateTime ReleaseDate, string CategoryName, string? PosterUrl, Dictionary<string, List<ShowTimeDto>> ShowTimesByCinema, bool IsFavorite);
public record GetMovieDetailsQuery(int Id, string? UserId = null) : IRequest<Result<MovieDetailsDto>>;

public class MovieCommandsHandler :
    IRequestHandler<CreateMovieCommand, Result<int>>,
    IRequestHandler<UpdateMovieCommand, Result>,
    IRequestHandler<DeleteMovieCommand, Result>
{
    private readonly IRepository<Movie> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public MovieCommandsHandler(IRepository<Movie> repository, IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
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

        if (request.PosterFile != null)
        {
            using var memoryStream = new System.IO.MemoryStream();
            await request.PosterFile.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();
            var extension = System.IO.Path.GetExtension(request.PosterFile.FileName);
            var fileName = await _fileStorageService.SaveFileAsync(content, extension, "posters");
            movie.Poster = new Poster { FileName = fileName, ContentType = request.PosterFile.ContentType };
        }

        await _repository.AddAsync(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(movie.Id);
    }

    public async Task<Result> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _repository.GetByIdAsync(request.Id, "Poster");
        if (movie == null) return Result.Failure(new Error("Movie.NotFound", "Movie not found."));

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.DurationMinutes = request.DurationMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.CategoryId = request.CategoryId;

        if (request.PosterFile != null)
        {
            if (movie.Poster != null)
            {
                _fileStorageService.DeleteFile(movie.Poster.FileName, "posters");
            }
            using var memoryStream = new System.IO.MemoryStream();
            await request.PosterFile.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();
            var extension = System.IO.Path.GetExtension(request.PosterFile.FileName);
            var fileName = await _fileStorageService.SaveFileAsync(content, extension, "posters");
            movie.Poster = new Poster { FileName = fileName, ContentType = request.PosterFile.ContentType };
        }

        _repository.Update(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _repository.GetByIdAsync(request.Id, "Poster");
        if (movie == null) return Result.Failure(new Error("Movie.NotFound", "Movie not found."));

        if (movie.Poster != null)
        {
            _fileStorageService.DeleteFile(movie.Poster.FileName, "posters");
        }

        _repository.Delete(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class MovieQueriesHandler :
    IRequestHandler<GetAllMoviesQuery, Result<List<MovieDto>>>,
    IRequestHandler<GetMovieByIdQuery, Result<MovieDto>>,
    IRequestHandler<GetMovieDetailsQuery, Result<MovieDetailsDto>>
{
    private readonly IRepository<Movie> _repository;
    private readonly IShowTimeRepository _showTimeRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public MovieQueriesHandler(IRepository<Movie> repository, IShowTimeRepository showTimeRepo, IMapper mapper, IMediator mediator)
    {
        _repository = repository;
        _showTimeRepo = showTimeRepo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<Result<List<MovieDto>>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
    {
        var allItems = await _repository.GetAllAsync("Category", "Poster");
        IEnumerable<Movie> items = allItems;
        
        if (request.CategoryId.HasValue)
            items = items.Where(m => m.CategoryId == request.CategoryId.Value);
            
        if (!string.IsNullOrEmpty(request.SearchTerm))
            items = items.Where(m => m.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) || m.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            
        return Result.Success(_mapper.Map<List<MovieDto>>(items.ToList()));
    }

    public async Task<Result<MovieDto>> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, "Category", "Poster");
        if (item == null) return Result.Failure<MovieDto>(new Error("Movie.NotFound", "Movie not found."));
        return Result.Success(_mapper.Map<MovieDto>(item));
    }

    public async Task<Result<MovieDetailsDto>> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
    {
        var movie = await _repository.GetByIdAsync(request.Id, "Category", "Poster");
        if (movie == null) return Result.Failure<MovieDetailsDto>(new Error("Movie.NotFound", "Movie not found."));

        var allShowtimes = await _showTimeRepo.GetAllAsync();
        var movieShowtimes = allShowtimes
            .Where(s => s.MovieId == request.Id && s.StartTime > DateTime.UtcNow)
            .OrderBy(s => s.StartTime)
            .ToList();

        var dtos = _mapper.Map<List<ShowTimeDto>>(movieShowtimes);
        var grouped = dtos.GroupBy(s => s.HallName ?? "Unknown Cinema").ToDictionary(g => g.Key, g => g.ToList());

        bool isFavorite = false;
        if (!string.IsNullOrEmpty(request.UserId))
        {
            var favoriteResult = await _mediator.Send(new CinemaBooking.Application.Features.Favorites.CheckIsFavoriteQuery(request.Id, request.UserId));
            if (favoriteResult.IsSuccess) isFavorite = favoriteResult.Value;
        }

        var dto = new MovieDetailsDto(
            movie.Id,
            movie.Title,
            movie.Description,
            movie.DurationMinutes,
            movie.ReleaseDate,
            movie.Category?.Name ?? "",
            movie.Poster != null ? $"/posters/{movie.Poster.FileName}" : null,
            grouped,
            isFavorite
        );

        return Result.Success(dto);
    }
}
