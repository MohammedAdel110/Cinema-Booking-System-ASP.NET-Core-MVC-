namespace CinemaBooking.Application.Features.Favorites;

using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record ToggleFavoriteCommand(int MovieId, string UserId) : IRequest<Result<bool>>;

public record CheckIsFavoriteQuery(int MovieId, string UserId) : IRequest<Result<bool>>;

public class FavoritesFeaturesHandler :
    IRequestHandler<ToggleFavoriteCommand, Result<bool>>,
    IRequestHandler<CheckIsFavoriteQuery, Result<bool>>
{
    private readonly IRepository<FavoriteMovie> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FavoritesFeaturesHandler(IRepository<FavoriteMovie> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ToggleFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existingFavorites = await _repository.GetAllAsync();
        var favorite = existingFavorites.FirstOrDefault(f => f.MovieId == request.MovieId && f.UserId == request.UserId);

        if (favorite != null)
        {
            _repository.Delete(favorite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(false); // Returning false means it's now removed
        }
        else
        {
            var newFavorite = new FavoriteMovie
            {
                MovieId = request.MovieId,
                UserId = request.UserId
            };
            await _repository.AddAsync(newFavorite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(true); // Returning true means it's now added
        }
    }

    public async Task<Result<bool>> Handle(CheckIsFavoriteQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId)) return Result.Success(false);

        var existingFavorites = await _repository.GetAllAsync();
        var exists = existingFavorites.Any(f => f.MovieId == request.MovieId && f.UserId == request.UserId);
        
        return Result.Success(exists);
    }
}
