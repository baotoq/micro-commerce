using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class CartProductMap
{
    public string CartId { get; set; }
    public Cart Cart { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }
}

public class CartProductMapConfiguration : IEntityTypeConfiguration<CartProductMapConfiguration>
{
    public void Configure(EntityTypeBuilder<CartProductMapConfiguration> builder)
    {
    }
}