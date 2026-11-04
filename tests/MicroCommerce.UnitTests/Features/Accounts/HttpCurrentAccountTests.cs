using System.Security.Claims;
using MicroCommerce.ApiService.Features.Accounts;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.UnitTests.Features.Accounts;

public class HttpCurrentAccountTests
{
    private static readonly Guid Subject = Guid.NewGuid();

    [Fact]
    public void Maps_the_token_claims_to_the_account()
    {
        var current = For(("sub", Subject.ToString()), ("name", "Bea Buyer"), ("email", "buyer@micro-commerce.test"));

        current.TryGet(out var account).ShouldBeTrue();

        account.Id.ShouldBe(AccountId.From(Subject));
        account.DisplayName.ShouldBe("Bea Buyer");
        account.Email.ShouldBe("buyer@micro-commerce.test");
        account.IsPlatformOperator.ShouldBeFalse();
    }

    [Fact]
    public void Display_name_falls_back_to_the_preferred_username()
    {
        var current = For(("sub", Subject.ToString()), ("preferred_username", "buyer"));

        current.TryGet(out var account).ShouldBeTrue();

        account.DisplayName.ShouldBe("buyer");
        account.Email.ShouldBeNull();
    }

    [Fact]
    public void Platform_operator_role_is_recognised()
    {
        var current = For(("sub", Subject.ToString()), ("name", "Olive"), ("roles", "other-role"), ("roles", "platform-operator"));

        current.TryGet(out var account).ShouldBeTrue();

        account.IsPlatformOperator.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-uuid")]
    public void Missing_or_non_uuid_subject_is_refused(string? subject)
    {
        var claims = subject is null ? [("name", "Bea")] : new[] { ("sub", subject), ("name", "Bea") };

        For(claims).TryGet(out _).ShouldBeFalse();
    }

    [Fact]
    public void Anonymous_request_has_no_account()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };

        new HttpCurrentAccount(accessor).TryGet(out _).ShouldBeFalse();
    }

    private static HttpCurrentAccount For(params (string Type, string Value)[] claims)
    {
        var identity = new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), "Bearer", "name", "roles");
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        return new HttpCurrentAccount(accessor);
    }
}
