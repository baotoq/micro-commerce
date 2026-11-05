using MicroCommerce.Domain.Accounts;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Accounts;

public class PickupAddressTests
{
    [Fact]
    public void Keeps_trimmed_street_district_and_province()
    {
        var address = new PickupAddress(" 12 Le Loi ", " District 1 ", " Ho Chi Minh City ");

        address.Street.ShouldBe("12 Le Loi");
        address.District.ShouldBe("District 1");
        address.Province.ShouldBe("Ho Chi Minh City");
    }

    [Theory]
    [InlineData("", "District 1", "Province")]
    [InlineData("Street", " ", "Province")]
    [InlineData("Street", "District 1", "")]
    public void Every_part_is_required(string street, string district, string province) =>
        Should.Throw<DomainRuleViolation>(() => new PickupAddress(street, district, province));

    [Fact]
    public void Street_is_at_most_200_and_district_and_province_at_most_100_characters()
    {
        _ = new PickupAddress(new string('s', 200), new string('d', 100), new string('p', 100));

        Should.Throw<DomainRuleViolation>(() => new PickupAddress(new string('s', 201), "District", "Province"));
        Should.Throw<DomainRuleViolation>(() => new PickupAddress("Street", new string('d', 101), "Province"));
        Should.Throw<DomainRuleViolation>(() => new PickupAddress("Street", "District", new string('p', 101)));
    }
}
