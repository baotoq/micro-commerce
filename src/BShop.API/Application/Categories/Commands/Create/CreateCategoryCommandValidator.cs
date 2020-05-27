using FluentValidation;

namespace BShop.API.Application.Categories.Commands.Create
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty()
                .MinimumLength(5);
        }
    }
}
