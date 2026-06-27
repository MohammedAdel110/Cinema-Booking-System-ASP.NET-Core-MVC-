namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.Categories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery());
        return View(result.IsSuccess ? result.Value : new List<CategoryDto>());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var command = new UpdateCategoryCommand(result.Value.Id, result.Value.Name, result.Value.Description);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id));
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Category deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Error.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
