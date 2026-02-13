using MediatR;
using MicroCommerce.ApiService.Common.Exceptions;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Commands.UploadAvatar;

public sealed record UploadAvatarCommand(Guid UserId, Stream ImageStream, string FileName) : IRequest<string>;

public sealed class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly ProfilesDbContext _context;
    private readonly IAvatarImageService _avatarImageService;

    public UploadAvatarCommandHandler(
        ProfilesDbContext context,
        IAvatarImageService avatarImageService)
    {
        _context = context;
        _avatarImageService = avatarImageService;
    }

    public async Task<string> Handle(
        UploadAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            throw new NotFoundException($"Profile for user '{request.UserId}' not found.");
        }

        // Delete old avatar if exists
        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl))
        {
            await _avatarImageService.DeleteAvatarAsync(profile.AvatarUrl, cancellationToken);
        }

        // Process and upload new avatar
        var newAvatarUrl = await _avatarImageService.ProcessAndUploadAvatarAsync(
            request.ImageStream,
            request.FileName,
            cancellationToken);

        profile.SetAvatar(newAvatarUrl);
        await _context.SaveChangesAsync(cancellationToken);

        return newAvatarUrl;
    }
}
