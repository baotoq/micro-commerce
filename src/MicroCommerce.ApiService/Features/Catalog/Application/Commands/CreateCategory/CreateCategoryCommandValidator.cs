using FluentValidation;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.CreateCategory;

/// <summary>
/// Validator for CreateCategoryCommand.
/// Runs in MediatR pipeline before handler executes.
/// </summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MinimumLength(2)
            .WithMessage("Category name must be at least 2 characters.")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.")
            .When(x => x.Description is not null);
    }
}
