using System.Net;
using MicroCommerce.ApiService.Features.Accounts;

namespace MicroCommerce.FunctionalTests.Features.Accounts;

public class MeEndpointTests(ApiFixture App) : TestBase<ApiFixture>
{
    [Fact]
    public async Task First_call_creates_an_account_from_the_token()
    {
        var buyer = TestActor.Buyer();

        var (rsp, me) = await App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        me.Id.ShouldNotBe(Guid.Empty);
        me.Email.ShouldBe(buyer.Email);
        me.DisplayName.ShouldBe(buyer.Name);
        me.IsPlatformOperator.ShouldBeFalse();
    }

    [Fact]
    public async Task Later_calls_with_the_same_subject_return_the_same_account()
    {
        var buyer = TestActor.Buyer();

        var (_, first) = await App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>();
        var (rsp, second) = await App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.ShouldBe(first);
    }

    [Fact]
    public async Task Concurrent_first_calls_with_the_same_subject_create_one_account()
    {
        var buyer = TestActor.Buyer();

        var results = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => App.ClientFor(buyer).GETAsync<GetMeEndpoint, MeResponse>()));

        results.ShouldAllBe(r => r.Response.StatusCode == HttpStatusCode.OK);
        results.Select(r => r.Result.Id).Distinct().Count().ShouldBe(1);
    }

    [Fact]
    public async Task Different_subjects_get_different_accounts()
    {
        var (_, first) = await App.ClientFor(TestActor.Buyer()).GETAsync<GetMeEndpoint, MeResponse>();
        var (_, second) = await App.ClientFor(TestActor.Buyer()).GETAsync<GetMeEndpoint, MeResponse>();

        second.Id.ShouldNotBe(first.Id);
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
    public async Task Token_signed_with_another_key_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer(), signingKey: TestIdentity.NewKey());

        var (rsp, _) = await App.CreateClient(c => c.Authenticate(token)).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tampered_token_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer());
        var parts = token.Split('.');
        var otherPayload = TestIdentity.TokenFor(TestActor.PlatformOperator()).Split('.')[1];
        var tampered = string.Join('.', parts[0], otherPayload, parts[2]);

        var (rsp, _) = await App.CreateClient(c => c.Authenticate(tampered)).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Expired_token_is_unauthorized()
    {
        // Well past the default five-minute clock skew.
        var token = TestIdentity.TokenFor(TestActor.Buyer(), expires: DateTime.UtcNow.AddHours(-1));

        var (rsp, _) = await App.CreateClient(c => c.Authenticate(token)).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Token_for_another_audience_is_unauthorized()
    {
        var token = TestIdentity.TokenFor(TestActor.Buyer(), audience: "some-other-api");

        var (rsp, _) = await App.CreateClient(c => c.Authenticate(token)).GETAsync<GetMeEndpoint, MeResponse>();

        rsp.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
