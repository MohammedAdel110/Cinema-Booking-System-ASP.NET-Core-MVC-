namespace CinemaBooking.Web.Controllers;

using CinemaBooking.Application.Features.Movies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CinemaBooking.Application.Features.Movies;
using CinemaBooking.Application.DTOs;
using System.Collections.Generic;

public class MoviesController : Controller
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(int? categoryId, string? searchTerm)
    {
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryId = categoryId;
        
        var categories = await _mediator.Send(new CinemaBooking.Application.Features.Categories.GetAllCategoriesQuery());
        ViewBag.Categories = categories.IsSuccess ? categories.Value : new List<CinemaBooking.Application.DTOs.CategoryDto>();

        var result = await _mediator.Send(new CinemaBooking.Application.Features.Movies.GetAllMoviesQuery(categoryId, searchTerm));
        if (result.IsSuccess)
        {
            return View(result.Value);
        }
        
        TempData["ErrorMessage"] = result.Error.Message;
        return View(new List<CinemaBooking.Application.DTOs.MovieDto>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var result = await _mediator.Send(new GetMovieDetailsQuery(id, userId));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _mediator.Send(new CinemaBooking.Application.Features.Favorites.ToggleFavoriteCommand(id, userId));
        
        if (result.IsSuccess)
        {
            return Json(new { success = true, isFavorite = result.Value });
        }

        return Json(new { success = false, message = result.Error.Message });
    }
}
