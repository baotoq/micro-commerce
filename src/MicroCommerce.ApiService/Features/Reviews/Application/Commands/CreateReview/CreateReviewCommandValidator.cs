using FluentValidation;

namespace MicroCommerce.ApiService.Features.Reviews.Application.Commands.CreateReview;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5 stars.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Review text is required.")
            .MinimumLength(10)
            .WithMessage("Review must be at least 10 characters.")
            .MaximumLength(1000)
            .WithMessage("Review must not exceed 1000 characters.");
    }
}
