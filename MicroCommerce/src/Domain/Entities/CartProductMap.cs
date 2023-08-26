using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class CartProductMap
{
    public string CartId { get; set; } = string.Empty;
    public Cart Cart { get; set; } = null!;

    public string ProductId { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;
}

public class CartProductMapConfiguration : IEntityTypeConfiguration<CartProductMapConfiguration>
{
    public void Configure(EntityTypeBuilder<CartProductMapConfiguration> builder)
    {
    }
}