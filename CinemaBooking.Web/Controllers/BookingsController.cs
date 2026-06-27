namespace CinemaBooking.Web.Controllers;

using CinemaBooking.Application.Features.Bookings.Commands;
using CinemaBooking.Application.Features.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
public class BookingsController : Controller
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> MyBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var result = await _mediator.Send(new GetUserBookingsQuery(userId));
        return View(result.IsSuccess ? result.Value : new());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var result = await _mediator.Send(new CancelBookingCommand(id, userId));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Booking cancelled successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error.Message;
        }

        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    public async Task<IActionResult> Book(int showTimeId)
    {
        var result = await _mediator.Send(new GetShowTimeDetailsForBookingQuery(showTimeId));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction("Index", "Movies");
        }
        
        ViewBag.ShowTimeDetails = result.Value;
        return View(new CreateBookingCommand("", showTimeId, 1));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(CreateBookingCommand command)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var fullCommand = command with { UserId = userId };
        
        if (!ModelState.IsValid)
        {
            var details = await _mediator.Send(new GetShowTimeDetailsForBookingQuery(command.ShowTimeId));
            ViewBag.ShowTimeDetails = details.IsSuccess ? details.Value : null;
            return View(fullCommand);
        }

        var result = await _mediator.Send(fullCommand);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Booking successful! Enjoy your movie.";
            return RedirectToAction(nameof(MyBookings));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        var retryDetails = await _mediator.Send(new GetShowTimeDetailsForBookingQuery(command.ShowTimeId));
        ViewBag.ShowTimeDetails = retryDetails.IsSuccess ? retryDetails.Value : null;
        return View(fullCommand);
    }
}
