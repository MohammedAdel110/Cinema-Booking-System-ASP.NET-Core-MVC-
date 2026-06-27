namespace CinemaBooking.Application.Features.Categories;

using AutoMapper;
using CinemaBooking.Application.DTOs;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Interfaces;
using CinemaBooking.Shared;
using FluentValidation;
using MediatR;

// COMMANDS
public record CreateCategoryCommand(string Name, string Description) : IRequest<Result<int>>;
public record UpdateCategoryCommand(int Id, string Name, string Description) : IRequest<Result>;
public record DeleteCategoryCommand(int Id) : IRequest<Result>;

// HANDLERS
public class CategoryCommandsHandler : 
    IRequestHandler<CreateCategoryCommand, Result<int>>,
    IRequestHandler<UpdateCategoryCommand, Result>,
    IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryCommandsHandler(IRepository<Category> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        await _repository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(category.Id);
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null) return Result.Failure(new Error("Category.NotFound", "Category not found."));

        category.Name = request.Name;
        category.Description = request.Description;
        
        _repository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id);
        if (category == null) return Result.Failure(new Error("Category.NotFound", "Category not found."));

        _repository.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

// VALIDATORS
public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Description).MaximumLength(1000);
    }
}

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Description).MaximumLength(1000);
    }
}

// QUERIES
public record GetAllCategoriesQuery : IRequest<Result<List<CategoryDto>>>;
public record GetCategoryByIdQuery(int Id) : IRequest<Result<CategoryDto>>;

// QUERY HANDLERS
public class CategoryQueriesHandler :
    IRequestHandler<GetAllCategoriesQuery, Result<List<CategoryDto>>>,
    IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public CategoryQueriesHandler(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync();
        return Result.Success(_mapper.Map<List<CategoryDto>>(items));
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id);
        if (item == null) return Result.Failure<CategoryDto>(new Error("Category.NotFound", "Category not found."));
        return Result.Success(_mapper.Map<CategoryDto>(item));
    }
}
