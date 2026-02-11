using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.AddAddress;

public sealed record AddAddressCommand(
    Guid UserId,
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool SetAsDefault) : IRequest<Guid>;

public sealed class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, Guid>
{
    private readonly ProfilesDbContext _context;

    public AddAddressCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        AddAddressCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        var addressId = profile.AddAddress(
            request.Name,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country,
            request.SetAsDefault);

        await _context.SaveChangesAsync(cancellationToken);

        return addressId.Value;
    }
}
