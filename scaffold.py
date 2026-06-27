import os

entities = [
    {"name": "Cinema", "fields": [("string", "Name"), ("string", "Location")]},
    {"name": "Hall", "fields": [("string", "Name"), ("int", "Capacity"), ("int", "CinemaId")]},
    {"name": "ShowTime", "fields": [("int", "MovieId"), ("int", "HallId"), ("DateTime", "StartTime"), ("decimal", "TicketPrice")]}
]

app_dir = r"C:\Users\Dell\.gemini\antigravity\scratch\CinemaBooking\CinemaBooking.Application\Features"
web_dir = r"C:\Users\Dell\.gemini\antigravity\scratch\CinemaBooking\CinemaBooking.Web\Areas\Admin"

def generate_cqrs(entity):
    name = entity["name"]
    plural = name + "s"
    fields = entity["fields"]
    
    cmd_args = ", ".join([f"{t} {f}" for t, f in fields])
    upd_args = "int Id, " + cmd_args
    
    init_obj = ",\n            ".join([f"{f} = request.{f}" for t, f in fields])
    upd_obj = "\n        ".join([f"item.{f} = request.{f};" for t, f in fields])
    
    validators = "\n        ".join([f"RuleFor(v => v.{f}).NotEmpty();" if t == "string" else f"RuleFor(v => v.{f}).GreaterThan(0);" for t, f in fields if f != "StartTime"])
    
    content = f"""namespace CinemaBooking.Application.Features.{plural};

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record Create{name}Command({cmd_args}) : IRequest<Result<int>>;
public record Update{name}Command({upd_args}) : IRequest<Result>;
public record Delete{name}Command(int Id) : IRequest<Result>;

public record GetAll{plural}Query : IRequest<Result<List<{name}Dto>>>;
public record Get{name}ByIdQuery(int Id) : IRequest<Result<{name}Dto>>;

public class {name}CommandHandler : 
    IRequestHandler<Create{name}Command, Result<int>>,
    IRequestHandler<Update{name}Command, Result>,
    IRequestHandler<Delete{name}Command, Result>
{{
    private readonly IRepository<{name}> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public {name}CommandHandler(IRepository<{name}> repository, IUnitOfWork unitOfWork)
    {{
        _repository = repository;
        _unitOfWork = unitOfWork;
    }}

    public async Task<Result<int>> Handle(Create{name}Command request, CancellationToken cancellationToken)
    {{
        var item = new {name} {{ {init_obj} }};
        await _repository.AddAsync(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }}

    public async Task<Result> Handle(Update{name}Command request, CancellationToken cancellationToken)
    {{
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("{name}.NotFound", "Not found."));

        {upd_obj}
        
        _repository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }}

    public async Task<Result> Handle(Delete{name}Command request, CancellationToken cancellationToken)
    {{
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure(new Error("{name}.NotFound", "Not found."));

        _repository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }}
}}

public class Create{name}Validator : AbstractValidator<Create{name}Command>
{{
    public Create{name}Validator()
    {{
        {validators}
    }}
}}

public class Update{name}Validator : AbstractValidator<Update{name}Command>
{{
    public Update{name}Validator()
    {{
        RuleFor(v => v.Id).GreaterThan(0);
        {validators}
    }}
}}

public class {name}QueriesHandler :
    IRequestHandler<GetAll{plural}Query, Result<List<{name}Dto>>>,
    IRequestHandler<Get{name}ByIdQuery, Result<{name}Dto>>
{{
    private readonly IRepository<{name}> _repository;
    private readonly IMapper _mapper;

    public {name}QueriesHandler(IRepository<{name}> repository, IMapper mapper)
    {{
        _repository = repository;
        _mapper = mapper;
    }}

    public async Task<Result<List<{name}Dto>>> Handle(GetAll{plural}Query request, CancellationToken cancellationToken)
    {{
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<{name}Dto>>(items));
    }}

    public async Task<Result<{name}Dto>> Handle(Get{name}ByIdQuery request, CancellationToken cancellationToken)
    {{
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure<{name}Dto>(new Error("{name}.NotFound", "Not found."));
        return Result.Success(_mapper.Map<{name}Dto>(item));
    }}
}}
"""
    os.makedirs(os.path.join(app_dir, plural), exist_ok=True)
    with open(os.path.join(app_dir, plural, f"{plural}Features.cs"), "w") as f:
        f.write(content)

def generate_controller(entity):
    name = entity["name"]
    plural = name + "s"
    
    content = f"""namespace CinemaBooking.Web.Areas.Admin.Controllers;

using CinemaBooking.Application.DTOs;
using CinemaBooking.Application.Features.{plural};
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class {plural}Controller : Controller
{{
    private readonly IMediator _mediator;

    public {plural}Controller(IMediator mediator)
    {{
        _mediator = mediator;
    }}

    public async Task<IActionResult> Index()
    {{
        var result = await _mediator.Send(new GetAll{plural}Query());
        return View(result.IsSuccess ? result.Value : new List<{name}Dto>());
    }}

    public IActionResult Create()
    {{
        return View();
    }}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Create{name}Command command)
    {{
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {{
            TempData["SuccessMessage"] = "{name} created successfully.";
            return RedirectToAction(nameof(Index));
        }}

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }}

    public async Task<IActionResult> Edit(int id)
    {{
        var result = await _mediator.Send(new Get{name}ByIdQuery(id));
        if (result.IsFailure)
        {{
            TempData["ErrorMessage"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }}

        var command = new Update{name}Command(result.Value.Id, { ", ".join([f"result.Value.{f[1]}" for f in entity["fields"]]) });
        return View(command);
    }}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Update{name}Command command)
    {{
        if (id != command.Id) return BadRequest();
        if (!ModelState.IsValid) return View(command);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {{
            TempData["SuccessMessage"] = "{name} updated successfully.";
            return RedirectToAction(nameof(Index));
        }}

        TempData["ErrorMessage"] = result.Error.Message;
        return View(command);
    }}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {{
        var result = await _mediator.Send(new Delete{name}Command(id));
        if (result.IsSuccess)
        {{
            TempData["SuccessMessage"] = "{name} deleted successfully.";
        }}
        else
        {{
            TempData["ErrorMessage"] = result.Error.Message;
        }}
        return RedirectToAction(nameof(Index));
    }}
}}
"""
    os.makedirs(os.path.join(web_dir, "Controllers"), exist_ok=True)
    with open(os.path.join(web_dir, "Controllers", f"{plural}Controller.cs"), "w") as f:
        f.write(content)

def generate_views(entity):
    name = entity["name"]
    plural = name + "s"
    fields = entity["fields"]
    
    os.makedirs(os.path.join(web_dir, "Views", plural), exist_ok=True)
    
    # Index
    th_tags = "".join([f"<th>{f[1]}</th>" for f in fields])
    td_tags = "".join([f"<td>@item.{f[1]}</td>" for f in fields])
    
    index = f"""@model IEnumerable<CinemaBooking.Application.DTOs.{name}Dto>
<div class="d-flex justify-content-between align-items-center mb-4">
    <h2>{plural}</h2>
    <a asp-action="Create" class="btn btn-primary"><i class="bi bi-plus-lg"></i> Add {name}</a>
</div>
<div class="card shadow-sm"><div class="card-body"><table class="table table-hover">
    <thead><tr><th>ID</th>{th_tags}<th class="text-end">Actions</th></tr></thead>
    <tbody>
        @foreach (var item in Model) {{
            <tr><td>@item.Id</td>{td_tags}
                <td class="text-end">
                    <a asp-action="Edit" asp-route-id="@item.Id" class="btn btn-sm btn-outline-secondary"><i class="bi bi-pencil"></i></a>
                    <form asp-action="Delete" asp-route-id="@item.Id" method="post" class="d-inline" onsubmit="return confirm('Delete?');">
                        <button type="submit" class="btn btn-sm btn-outline-danger"><i class="bi bi-trash"></i></button>
                    </form>
                </td>
            </tr>
        }}
    </tbody>
</table></div></div>
"""
    with open(os.path.join(web_dir, "Views", plural, "Index.cshtml"), "w") as f:
        f.write(index)

    # Create
    inputs = "".join([f"""
    <div class="mb-3">
        <label asp-for="{f[1]}" class="form-label"></label>
        <input asp-for="{f[1]}" class="form-control" type="{ 'number' if f[0] == 'int' else 'text' if f[0] == 'string' else 'datetime-local' if f[0] == 'DateTime' else 'number' }" step="any" />
        <span asp-validation-for="{f[1]}" class="text-danger"></span>
    </div>""" for f in fields])

    create = f"""@model CinemaBooking.Application.Features.{plural}.Create{name}Command
<h2>Create {name}</h2>
<form asp-action="Create" method="post">
    {inputs}
    <button type="submit" class="btn btn-primary">Save</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
"""
    with open(os.path.join(web_dir, "Views", plural, "Create.cshtml"), "w") as f:
        f.write(create)

    # Edit
    edit = f"""@model CinemaBooking.Application.Features.{plural}.Update{name}Command
<h2>Edit {name}</h2>
<form asp-action="Edit" method="post">
    <input type="hidden" asp-for="Id" />
    {inputs}
    <button type="submit" class="btn btn-primary">Save</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>
"""
    with open(os.path.join(web_dir, "Views", plural, "Edit.cshtml"), "w") as f:
        f.write(edit)

for e in entities:
    generate_cqrs(e)
    generate_controller(e)
    generate_views(e)

# Also generate Create/Edit for Category
cat = {"name": "Category", "fields": [("string", "Name"), ("string", "Description")]}
generate_views(cat)

print("Scaffolding complete.")
