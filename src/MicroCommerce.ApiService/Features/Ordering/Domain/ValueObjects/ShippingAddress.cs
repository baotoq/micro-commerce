namespace MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

/// <summary>
/// Shipping address value object capturing all required delivery information.
/// </summary>
public sealed record ShippingAddress
{
    public string Name { get; }
    public string Email { get; }
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }

    public ShippingAddress(string name, string email, string street, string city, string state, string zipCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(zipCode);

        Name = name;
        Email = email;
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }
}
