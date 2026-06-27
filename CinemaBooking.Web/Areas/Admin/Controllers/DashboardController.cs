namespace CinemaBooking.Web.Areas.Admin.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new CinemaBooking.Application.Features.Admin.GetDashboardStatisticsQuery());
        return View(result.IsSuccess ? result.Value : new CinemaBooking.Application.Features.Admin.DashboardStatisticsDto(0,0,0,0));
    }
}
