using Ardalis.GuardClauses;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.DeliveryOptions;

public class UpdateDeliveryOption
{
    public record Command : IRequest<Unit>
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Unit>
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            // Arrange
            var deliveryOption = await _context.DeliveryOptions
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (deliveryOption is null)
            {
                throw new NotFoundException(request.Id.ToString(), "Delivery option not found");
            }

            // Act
            deliveryOption.Name = request.Name;

            _context.DeliveryOptions.Update(deliveryOption);
            await _context.SaveChangesAsync(cancellationToken);

            // Assert
            return Unit.Value;
        }
    }
}
