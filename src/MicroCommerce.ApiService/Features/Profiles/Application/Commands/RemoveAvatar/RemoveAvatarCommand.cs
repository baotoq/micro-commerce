using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.RemoveAvatar;

public sealed record RemoveAvatarCommand(Guid UserId) : IRequest;

public sealed class RemoveAvatarCommandHandler : IRequestHandler<RemoveAvatarCommand>
{
    private readonly ProfilesDbContext _context;
    private readonly IAvatarImageService _avatarImageService;

    public RemoveAvatarCommandHandler(
        ProfilesDbContext context,
        IAvatarImageService avatarImageService)
    {
        _context = context;
        _avatarImageService = avatarImageService;
    }

    public async Task Handle(
        RemoveAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        // Delete avatar from blob storage if exists
        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
        {
            await _avatarImageService.DeleteAvatarAsync(profile.AvatarUrl, cancellationToken);
        }

        profile.RemoveAvatar();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
