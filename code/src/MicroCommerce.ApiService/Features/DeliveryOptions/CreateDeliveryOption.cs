using FluentValidation;
using MediatR;
using MicroCommerce.ApiService.Domain.Entities;
using MicroCommerce.ApiService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.DeliveryOptions;

public class CreateDeliveryOption
{
    public record Command : IRequest<Response>
    {
        public required string Name { get; set; }
        public required decimal MinimumSpending { get; set; }
        public required decimal Fee { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.MinimumSpending).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Fee).GreaterThanOrEqualTo(0);
        }
    }

    public record Response(Guid Id);

    public class Handler(ApplicationDbContext context) : IRequestHandler<Command, Response>
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var deliveryOption = new DeliveryOption
            {
                Name = request.Name,
                MinimumSpending = request.MinimumSpending,
                Fee = request.Fee
            };

            _context.DeliveryOptions.Add(deliveryOption);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response(deliveryOption.Id);
        }
    }
}
