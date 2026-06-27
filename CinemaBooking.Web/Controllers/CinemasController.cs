namespace CinemaBooking.Web.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.Cinemas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        if (result.IsSuccess)
        {
            return View(result.Value);
        }
        
        TempData["ErrorMessage"] = result.Error.Message;
        return View(new List<CinemaDto>());
    }
}
