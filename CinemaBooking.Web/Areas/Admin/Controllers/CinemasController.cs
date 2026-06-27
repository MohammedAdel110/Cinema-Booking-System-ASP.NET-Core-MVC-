namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.Cinemas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CinemasController : Controller
{
    private readonly IMediator _mediator;

    public CinemasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllCinemasQuery());
        return View(result.IsSuccess ? result.Value : new List<CinemaDto>());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCinemaCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Cinema created successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetCinemaByIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var command = new UpdateCinemaCommand(result.Value.Id, result.Value.Name, result.Value.Location);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCinemaCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Cinema updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCinemaCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Cinema deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
