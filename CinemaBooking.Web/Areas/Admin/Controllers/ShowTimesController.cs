namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.ShowTimes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using CinemaBooking.Application.Features.Movies;
using CinemaBooking.Application.Features.Halls;
using System.Collections.Generic;
using System.Threading.Tasks;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ShowTimesController : Controller
{
    private readonly IMediator _mediator;

    public ShowTimesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllShowTimesQuery());
        return View(result.IsSuccess ? result.Value : new List<ShowTimeDto>());
    }

    private async Task LoadMoviesAndHalls()
    {
        var moviesResult = await _mediator.Send(new GetAllMoviesQuery());
        var hallsResult = await _mediator.Send(new GetAllHallsQuery());
        
        ViewBag.Movies = new SelectList(moviesResult.IsSuccess ? moviesResult.Value : new List<MovieDto>(), "Id", "Title");
        ViewBag.Halls = new SelectList(hallsResult.IsSuccess ? hallsResult.Value : new List<HallDto>(), "Id", "Name");
    }

    public async Task<IActionResult> Create()
    {
        await LoadMoviesAndHalls();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateShowTimeCommand command)
    {
        if (!ModelState.IsValid) 
        {
            await LoadMoviesAndHalls();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "ShowTime created successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        await LoadMoviesAndHalls();
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetShowTimeByIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var command = new UpdateShowTimeCommand(result.Value.Id, result.Value.MovieId, result.Value.HallId, result.Value.StartTime, result.Value.TicketPrice);
        await LoadMoviesAndHalls();
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateShowTimeCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) 
        {
            await LoadMoviesAndHalls();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "ShowTime updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        await LoadMoviesAndHalls();
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteShowTimeCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "ShowTime deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
