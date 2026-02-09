using MicroCommerce.BuildingBlocks.Common;

namespace MicroCommerce.ApiService.Features.Profiles.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public AddressId Id { get; private set; }
    public string Name { get; private set; }
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string Country { get; private set; }
    public bool IsDefault { get; private set; }

    // EF Core constructor
    private Address()
    {
        Id = null!;
        Name = string.Empty;
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
        Country = string.Empty;
    }

    private Address(AddressId id, string name, string street, string city, string state, string zipCode, string country, bool isDefault)
    {
        Id = id;
        Name = name;
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        IsDefault = isDefault;
    }

    public static Address Create(string name, string street, string city, string state, string zipCode, string country)
    {
        return new Address(
            AddressId.New(),
            name.Trim(),
            street.Trim(),
            city.Trim(),
            state.Trim(),
            zipCode.Trim(),
            country.Trim(),
            false
        );
    }

    internal void SetAsDefault()
    {
        IsDefault = true;
    }

    internal void ClearDefault()
    {
        IsDefault = false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
