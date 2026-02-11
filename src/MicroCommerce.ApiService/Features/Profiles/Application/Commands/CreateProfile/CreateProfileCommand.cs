using MediatR;
using MicroCommerce.ApiService.Features.Profiles.Domain.Entities;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.CreateProfile;

public sealed record CreateProfileCommand(Guid UserId, string DisplayName) : IRequest<Guid>;

public sealed class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, Guid>
{
    private readonly ProfilesDbContext _context;

    public CreateProfileCommandHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        CreateProfileCommand request,
        CancellationToken cancellationToken)
    {
        // Check if profile exists (idempotent operation)
        var existingProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (existingProfile is not null)
        {
            return existingProfile.Id.Value;
        }

        // Create new profile
        var profile = UserProfile.Create(request.UserId, request.DisplayName);
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return profile.Id.Value;
    }
}
