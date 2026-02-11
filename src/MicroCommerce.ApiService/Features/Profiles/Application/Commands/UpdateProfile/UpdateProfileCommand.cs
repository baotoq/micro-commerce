using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, string DisplayName) : IRequest;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand>
{
    private readonly ProfilesDbContext _context;

    public UpdateProfileCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        profile.UpdateDisplayName(request.DisplayName);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
