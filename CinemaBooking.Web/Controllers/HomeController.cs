namespace CinemaBooking.Web.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using CinemaBooking.Application.Features.Movies;
using CinemaBooking.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllMoviesQuery());
        return View(result.IsSuccess ? result.Value : new List<MovieDto>());
    }
}
