using Ardalis.GuardClauses;
using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.DeliveryOptions;

public class DeleteDeliveryOption
{
    public record Command : IRequest<Unit>
    {
        public required Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Unit>
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var deliveryOption = await _context.DeliveryOptions
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (deliveryOption is null)
            {
                throw new NotFoundException(request.Id.ToString(), "Delivery option not found");
            }

            _context.DeliveryOptions.Remove(deliveryOption);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
