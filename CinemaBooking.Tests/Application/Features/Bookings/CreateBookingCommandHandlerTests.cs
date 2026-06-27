namespace CinemaBooking.Tests.Application.Features.Bookings;

using CinemaBooking.Application.Features.Bookings.Commands;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CreateBookingCommandHandlerTests
{
    private readonly Mock<IShowTimeRepository> _showTimeRepoMock;
    private readonly Mock<IRepository<Booking>> _bookingRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBookingCommandHandler _handler;

    public CreateBookingCommandHandlerTests()
    {
        _showTimeRepoMock = new Mock<IShowTimeRepository>();
        _bookingRepoMock = new Mock<IRepository<Booking>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateBookingCommandHandler(
            _showTimeRepoMock.Object,
            _bookingRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ShowTimeNotFound()
    {
        // Arrange
        var command = new CreateBookingCommand("user123", 1, 2);
        _showTimeRepoMock.Setup(repo => repo.GetShowTimeWithDetailsAsync(1))
            .ReturnsAsync((ShowTime)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ShowTimeStarted()
    {
        // Arrange
        var command = new CreateBookingCommand("user123", 1, 2);
        var showTime = new ShowTime { StartTime = DateTime.UtcNow.AddMinutes(-10) };
        
        _showTimeRepoMock.Setup(repo => repo.GetShowTimeWithDetailsAsync(1))
            .ReturnsAsync(showTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Started");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_NotEnoughSeats()
    {
        // Arrange
        var command = new CreateBookingCommand("user123", 1, 3); // User wants 3 seats
        
        var hall = new Hall { Capacity = 10 };
        var showTime = new ShowTime 
        { 
            StartTime = DateTime.UtcNow.AddDays(1),
            Hall = hall
        };
        
        // 8 seats already booked
        var existingBooking = new Booking { IsCanceled = false };
        for(int i=0; i<8; i++) existingBooking.Items.Add(new BookingItem());
        showTime.Bookings.Add(existingBooking);

        _showTimeRepoMock.Setup(repo => repo.GetShowTimeWithDetailsAsync(1))
            .ReturnsAsync(showTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.Capacity");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_BookingIsValid()
    {
        // Arrange
        var command = new CreateBookingCommand("user123", 1, 2);
        
        var hall = new Hall { Capacity = 10 };
        var showTime = new ShowTime 
        { 
            Id = 1,
            StartTime = DateTime.UtcNow.AddDays(1),
            Hall = hall,
            TicketPrice = 15.0m
        };

        _showTimeRepoMock.Setup(repo => repo.GetShowTimeWithDetailsAsync(1))
            .ReturnsAsync(showTime);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _bookingRepoMock.Verify(repo => repo.AddAsync(It.Is<Booking>(b => 
            b.UserId == "user123" && 
            b.ShowTimeId == 1 &&
            b.TotalAmount == 30.0m &&
            b.Items.Count == 2)), Times.Once);
            
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
