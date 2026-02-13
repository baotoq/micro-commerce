using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.DeleteAddress;

public sealed record DeleteAddressCommand(Guid UserId, Guid AddressId) : IRequest;

public sealed class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand>
{
    private readonly ProfilesDbContext _context;

    public DeleteAddressCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteAddressCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        profile.DeleteAddress(new AddressId(request.AddressId));
        await _context.SaveChangesAsync(cancellationToken);
    }
}
