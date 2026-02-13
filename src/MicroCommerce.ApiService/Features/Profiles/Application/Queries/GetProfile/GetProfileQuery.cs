using MediatR;
using MicroCommerce.ApiService.Features.Profiles.Domain.Entities;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Profiles.Application.Queries.GetProfile;

public sealed record GetProfileQuery(Guid UserId) : IRequest<ProfileDto?>;

public sealed record ProfileDto(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    List<AddressDto> Addresses,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AddressDto(
    Guid Id,
    string Name,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool IsDefault);

public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto?>
{
    private readonly ProfilesDbContext _context;

    public GetProfileQueryHandler(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto?> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .Include(p => p.Addresses)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        // Auto-create profile if not found
        if (profile is null)
        {
            profile = UserProfile.Create(request.UserId, "User");
            _context.UserProfiles.Add(profile);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Profile creation failed - return null
                return null;
            }
        }

        return new ProfileDto(
            profile.Id.Value,
            profile.UserId,
            profile.DisplayName.Value,
            profile.AvatarUrl,
            profile.Addresses.Select(a => new AddressDto(
                a.Id.Value,
                a.Name,
                a.Street,
                a.City,
                a.State,
                a.ZipCode,
                a.Country,
                a.IsDefault)).ToList(),
            profile.CreatedAt,
            profile.UpdatedAt);
    }
}
