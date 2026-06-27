namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.Halls;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBooking.Application.Features.Cinemas;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HallsController : Controller
{
    private readonly IMediator _mediator;

    public HallsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllHallsQuery());
        return View(result.IsSuccess ? result.Value : new List<HallDto>());
    }

    public async Task<IActionResult> Create()
    {
        await LoadCinemas();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHallCommand command)
    {
        if (!ModelState.IsValid) 
        {
            await LoadCinemas();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Hall created successfully.";
            return RedirectToAction(nameof(Index));
        }

        await LoadCinemas();
        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetHallByIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        await LoadCinemas();
        var command = new UpdateHallCommand(result.Value.Id, result.Value.Name, result.Value.Capacity, result.Value.CinemaId);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateHallCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) 
        {
            await LoadCinemas();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Hall updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        await LoadCinemas();
        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteHallCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Hall deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCinemas()
    {
        var cinemas = await _mediator.Send(new GetAllCinemasQuery());
        ViewBag.Cinemas = cinemas.IsSuccess ? new SelectList(cinemas.Value, "Id", "Name") : new SelectList(new List<CinemaDto>(), "Id", "Name");
    }
}
