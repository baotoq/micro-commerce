using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid UserId,
    Guid AddressId,
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : IRequest;

public sealed class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand>
{
    private readonly ProfilesDbContext _context;

    public UpdateAddressCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        profile.UpdateAddress(
            new AddressId(request.AddressId),
            request.Name,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
