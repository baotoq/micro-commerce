using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid UserId, Guid AddressId) : IRequest;

public sealed class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand>
{
    private readonly ProfilesDbContext _context;

    public SetDefaultAddressCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        SetDefaultAddressCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        profile.SetDefaultAddress(new AddressId(request.AddressId));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
