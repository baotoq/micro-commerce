using MediatR;
using MicroCommerce.Catalog.Application.Customers.Dtos;
using MicroCommerce.Catalog.Application.Customers.Events;
using MicroCommerce.Catalog.Application.Persistence;
using MicroCommerce.Catalog.Domain.Common;
using MicroCommerce.Catalog.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MicroCommerce.Catalog.Application.Customers.Commands;

public record CreateCustomerCommand(
    string Name,
    string Email,
    string City) : IRequest<Result<CustomerDto>>;

public class CreateCustomerHandler(AppDbContext db, IPublisher publisher, IClock clock) : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private const string PostgresUniqueViolation = "23505";

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var email = Email.From(request.Email);

        // Optimistic pre-check; unique index on Email is the authoritative guarantee
        // via the DbUpdateException catch below.
        if (await db.Customers.AsNoTracking().AnyAsync(c => c.Email == email, ct))
            return Result<CustomerDto>.Conflict("CUSTOMER_EMAIL_DUPLICATE", $"Customer with email '{email.Value}' already exists.");

        // IClock instead of DateTimeOffset.UtcNow — deterministic demo clock (see plan D1).
        var customer = new Customer(request.Name, email, request.City, clock.Now);

        db.Customers.Add(customer);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation })
        {
            return Result<CustomerDto>.Conflict("CUSTOMER_EMAIL_DUPLICATE", $"Customer with email '{email.Value}' already exists.");
        }

        var dto = CustomerMapping.ToDto(customer, 0, 0m, null, null);
        await publisher.Publish(new CustomerCreatedEvent(dto.Email), ct);
        return Result<CustomerDto>.Success(dto);
    }
}
