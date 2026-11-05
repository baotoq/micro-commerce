using MicroCommerce.ApiService.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MicroCommerce.UnitTests.Infrastructure;

public class ProblemsTests
{
    [Theory]
    [InlineData(Problems.MerchantExists)]
    [InlineData(Problems.ShopNameTaken)]
    [InlineData(Problems.ShopVersionConflict)]
    [InlineData(Problems.ProductVersionConflict)]
    [InlineData(Problems.ProductRemoved)]
    [InlineData(Problems.CategoryHasProducts)]
    [InlineData(Problems.CategoryHasChildren)]
    [InlineData(Problems.CategoryNameTaken)]
    [InlineData(Problems.ImageLimitReached)]
    public void Every_conflict_type_is_a_409_with_that_type_and_a_title(string type)
    {
        var problem = Problems.Conflict(type).ShouldBeOfType<ProblemHttpResult>().ProblemDetails;

        problem.Status.ShouldBe(409);
        problem.Type.ShouldBe(type);
        problem.Title.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Merchant_required_is_a_403()
    {
        ProblemDetails problem = Problems.MerchantRequiredResult().ShouldBeOfType<ProblemHttpResult>().ProblemDetails;

        problem.Status.ShouldBe(403);
        problem.Type.ShouldBe(Problems.MerchantRequired);
    }
}
