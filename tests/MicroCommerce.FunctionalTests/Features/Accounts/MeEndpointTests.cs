using System.Net;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.FunctionalTests.Features.Accounts;

public class MeEndpointTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Fact]
    public async Task Buyer_gets_their_account_from_the_token()
    {
        var buyer = TestActor.Buyer();

        var (rsp, me) = await App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.AccountId.ShouldBe(Guid.Parse(buyer.Subject));
        me.DisplayName.ShouldBe(buyer.Name);
        me.Email.ShouldBe(buyer.Email);
        me.IsPlatformOperator.ShouldBeFalse();
    }

    [Fact]
    public async Task Platform_operator_role_is_reported()
    {
        var (rsp, me) = await App.ClientFor(TestActor.PlatformOperator()).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.IsPlatformOperator.ShouldBeTrue();
    }

    [Fact]
    public async Task Missing_token_is_unauthorized()
    {
        var (rsp, _) = await App.Client.GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Expired_token_is_unauthorized()
    {
        // Well past the default five-minute clock skew.
        var token = TestIdentity.TokenFor(TestActor.Buyer(), expires: DateTime.UtcNow.AddHours(-1));

        var (rsp, _) = await App.ClientFor(token).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer(), signingKey: TestIdentity.NewKey());

        var (rsp, _) = await App.ClientFor(token).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tampered_token_is_unauthorized()
    {
        var parts = TestIdentity.TokenFor(TestActor.Buyer()).Split('.');
        var otherPayload = TestIdentity.TokenFor(TestActor.PlatformOperator()).Split('.')[1];
        var tampered = string.Join('.', parts[0], otherPayload, parts[2]);

        var (rsp, _) = await App.ClientFor(tampered).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Token_for_another_audience_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer(), audience: "other");

        var (rsp, _) = await App.ClientFor(token).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Token_with_a_non_uuid_subject_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer() with { Subject = "not-a-uuid" });

        var (rsp, _) = await App.ClientFor(token).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
