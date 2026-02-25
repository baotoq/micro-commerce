using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Profiles.Application.Queries.GetProfile;
using MicroCommerce.ApiService.Features.Profiles.Infrastructure;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Profiles;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class ProfilesEndpointsTests : IntegrationTestBase
{
    public ProfilesEndpointsTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await ResetDatabase(typeof(ProfilesDbContext));
    }

    [Fact]
    public async Task GetMyProfile_NewUser_AutoCreatesAndReturnsProfile()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/profiles/me");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        ProfileDto? profile = await response.Content.ReadFromJsonAsync<ProfileDto>();
        profile.Should().NotBeNull();
        profile!.UserId.Should().Be(userId);
        profile.Addresses.Should().BeEmpty();
    }
}
