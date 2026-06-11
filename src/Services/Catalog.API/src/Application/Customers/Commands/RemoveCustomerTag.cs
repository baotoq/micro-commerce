using MediatR;
using MicroCommerce.Catalog.Application.Customers.Dtos;
using MicroCommerce.Catalog.Application.Customers.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.Catalog.Application.Customers.Commands;

public record RemoveCustomerTagCommand(string Email, string Tag) : IRequest<Result<CustomerDto>>;

public class RemoveCustomerTagHandler(AppDbContext db, IPublisher publisher) : IRequestHandler<RemoveCustomerTagCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(RemoveCustomerTagCommand request, CancellationToken ct)
    {
        var email = Email.From(request.Email);
        // AsTracking() is required: DbContext default is NoTracking, so mutating the loaded
        // entity without it would make SaveChanges a silent no-op
        // (project memory: project_ef_notracking_mutating_handlers).
        var customer = await db.Customers
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);
        if (customer is null)
            return Result<CustomerDto>.Conflict("CUSTOMER_NOT_FOUND", $"Customer '{email.Value}' was not found.");

        // RemoveTag is a no-op when the tag is absent.
        customer.RemoveTag(request.Tag);

        await db.SaveChangesAsync(ct);

        var dto = CustomerMapping.ToDto(customer, 0, 0m, null, null);
        await publisher.Publish(new CustomerUpdatedEvent(email.Value), ct);
        return Result<CustomerDto>.Success(dto);
    }
}
