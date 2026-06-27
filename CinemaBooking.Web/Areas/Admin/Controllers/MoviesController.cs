namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.Categories;
using CinemaBooking.Application.Features.Movies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MoviesController : Controller
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllMoviesQuery());
        return View(result.IsSuccess ? result.Value : new List<MovieDto>());
    }

    private async Task PopulateCategories()
    {
        var categories = await _mediator.Send(new GetAllCategoriesQuery());
        ViewBag.Categories = new SelectList(categories.IsSuccess ? categories.Value : new List<CategoryDto>(), "Id", "Name");
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategories();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMovieCommand command)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategories();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Movie created successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        await PopulateCategories();
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetMovieByIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var command = new UpdateMovieCommand(
            result.Value.Id, 
            result.Value.Title, 
            result.Value.Description, 
            result.Value.DurationMinutes, 
            result.Value.ReleaseDate, 
            result.Value.CategoryId, 
            null);
            
        await PopulateCategories();
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMovieCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateCategories();
            return View(command);
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Movie updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        await PopulateCategories();
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteMovieCommand(id));
        if (result.IsSuccess) TempData["SuccessMessage"] = "Movie deleted successfully.";
        else TempData["ErrorMessage"] = result.Error.Message;
        return RedirectToAction(nameof(Index));
    }
}
