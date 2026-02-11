using FluentValidation;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.AddAddress;

public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(50)
            .WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street is required.")
            .MaximumLength(200)
            .WithMessage("Street must not exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required.")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required.")
            .MaximumLength(50)
            .WithMessage("State must not exceed 50 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("ZIP code is required.")
            .MaximumLength(20)
            .WithMessage("ZIP code must not exceed 20 characters.")
            .Matches(@"^[0-9a-zA-Z\s\-]+$")
            .WithMessage("ZIP code can only contain letters, numbers, spaces, and hyphens.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required.")
            .MaximumLength(100)
            .WithMessage("Country must not exceed 100 characters.");
    }
}
