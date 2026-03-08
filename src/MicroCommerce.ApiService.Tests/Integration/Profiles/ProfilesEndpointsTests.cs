using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Profiles;
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

    [Fact]
    public async Task UpdateProfile_ValidDisplayName_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile first
        await client.GetAsync("/api/profiles/me");

        UpdateProfileRequest request = new("Updated Name");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync("/api/profiles/me", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        ProfileDto? profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        profile!.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UploadAvatar_ValidImage_Returns200()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile first
        await client.GetAsync("/api/profiles/me");

        using var content = new MultipartFormDataContent();
        byte[] fakeImage = new byte[100];
        content.Add(new ByteArrayContent(fakeImage), "file", "avatar.png");

        // Act
        HttpResponseMessage response = await client.PostAsync("/api/profiles/me/avatar", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UploadAvatarResult? result = await response.Content.ReadFromJsonAsync<UploadAvatarResult>();
        result.Should().NotBeNull();
        result!.AvatarUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RemoveAvatar_ExistingProfile_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile first
        await client.GetAsync("/api/profiles/me");

        // Act
        HttpResponseMessage response = await client.DeleteAsync("/api/profiles/me/avatar");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AddAddress_ValidAddress_Returns201()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile first
        await client.GetAsync("/api/profiles/me");

        AddAddressRequest request = new(
            "Home",
            "123 Main St",
            "Seattle",
            "WA",
            "98101",
            "US");

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/profiles/me/addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify address was added
        ProfileDto? profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        profile!.Addresses.Should().HaveCount(1);
        profile.Addresses.First().Name.Should().Be("Home");
        profile.Addresses.First().Street.Should().Be("123 Main St");
    }

    [Fact]
    public async Task UpdateAddress_ExistingAddress_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile and add address
        await client.GetAsync("/api/profiles/me");
        AddAddressRequest addRequest = new("Home", "123 Main St", "Seattle", "WA", "98101", "US");
        await client.PostAsJsonAsync("/api/profiles/me/addresses", addRequest);

        ProfileDto? profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        Guid addressId = profile!.Addresses.First().Id;

        UpdateAddressRequest updateRequest = new("Office", "456 Work Ave", "Portland", "OR", "97201", "US");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/profiles/me/addresses/{addressId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        profile!.Addresses.First().Name.Should().Be("Office");
        profile.Addresses.First().Street.Should().Be("456 Work Ave");
    }

    [Fact]
    public async Task UpdateAddress_NonExistentAddress_Returns404()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile
        await client.GetAsync("/api/profiles/me");

        UpdateAddressRequest updateRequest = new("Office", "456 Work Ave", "Portland", "OR", "97201", "US");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/profiles/me/addresses/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAddress_ExistingAddress_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile and add address
        await client.GetAsync("/api/profiles/me");
        AddAddressRequest addRequest = new("Home", "123 Main St", "Seattle", "WA", "98101", "US");
        await client.PostAsJsonAsync("/api/profiles/me/addresses", addRequest);

        ProfileDto? profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        Guid addressId = profile!.Addresses.First().Id;

        // Act
        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/profiles/me/addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        profile!.Addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAddress_NonExistentAddress_Returns404()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile
        await client.GetAsync("/api/profiles/me");

        // Act
        HttpResponseMessage response = await client.DeleteAsync(
            $"/api/profiles/me/addresses/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetDefaultAddress_ExistingAddress_Returns204()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Auto-create profile and add two addresses
        await client.GetAsync("/api/profiles/me");
        await client.PostAsJsonAsync("/api/profiles/me/addresses",
            new AddAddressRequest("Home", "123 Main St", "Seattle", "WA", "98101", "US"));
        await client.PostAsJsonAsync("/api/profiles/me/addresses",
            new AddAddressRequest("Office", "456 Work Ave", "Portland", "OR", "97201", "US"));

        ProfileDto? profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        Guid secondAddressId = profile!.Addresses.Last().Id;

        // Act
        HttpResponseMessage response = await client.PatchAsync(
            $"/api/profiles/me/addresses/{secondAddressId}/default", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the second address is now default
        profile = await client.GetFromJsonAsync<ProfileDto>("/api/profiles/me");
        profile!.Addresses.Single(a => a.Id == secondAddressId).IsDefault.Should().BeTrue();
    }
}
