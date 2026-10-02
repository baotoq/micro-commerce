using FastEndpoints;
using FluentValidation;
using MicroCommerce.ApiService.Data;

namespace MicroCommerce.ApiService.Features.Products;

public record CreateProductRequest(string Name, decimal Price);

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductEndpoint(AppDbContext db) : Endpoint<CreateProductRequest, ProductResponse>
{
    public override void Configure()
    {
        Post("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product { Name = req.Name, Price = req.Price };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        await Send.CreatedAtAsync<GetProductEndpoint>(
            new { id = product.Id }, ProductResponse.From(product), cancellation: ct);
    }
}
