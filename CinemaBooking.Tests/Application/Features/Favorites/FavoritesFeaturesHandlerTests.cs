namespace CinemaBooking.Tests.Application.Features.Favorites;

using CinemaBooking.Application.Features.Favorites;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class FavoritesFeaturesHandlerTests
{
    private readonly Mock<IRepository<FavoriteMovie>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly FavoritesFeaturesHandler _handler;

    public FavoritesFeaturesHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<FavoriteMovie>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new FavoritesFeaturesHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ToggleFavoriteCommand_Should_AddFavorite_When_NotExists()
    {
        // Arrange
        var command = new ToggleFavoriteCommand(1, "user1");
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<FavoriteMovie>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Returns true when added
        _repositoryMock.Verify(r => r.AddAsync(It.Is<FavoriteMovie>(f => f.MovieId == 1 && f.UserId == "user1")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ToggleFavoriteCommand_Should_RemoveFavorite_When_Exists()
    {
        // Arrange
        var command = new ToggleFavoriteCommand(1, "user1");
        var existingFavorite = new FavoriteMovie { MovieId = 1, UserId = "user1" };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<FavoriteMovie> { existingFavorite });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse(); // Returns false when removed
        _repositoryMock.Verify(r => r.Delete(existingFavorite), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CheckIsFavoriteQuery_Should_ReturnTrue_When_Exists()
    {
        // Arrange
        var query = new CheckIsFavoriteQuery(1, "user1");
        var existingFavorite = new FavoriteMovie { MovieId = 1, UserId = "user1" };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<FavoriteMovie> { existingFavorite });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CheckIsFavoriteQuery_Should_ReturnFalse_When_NotExists()
    {
        // Arrange
        var query = new CheckIsFavoriteQuery(1, "user1");
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<FavoriteMovie>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CheckIsFavoriteQuery_Should_ReturnFalse_When_UserIdIsEmpty()
    {
        // Arrange
        var query = new CheckIsFavoriteQuery(1, "");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}
