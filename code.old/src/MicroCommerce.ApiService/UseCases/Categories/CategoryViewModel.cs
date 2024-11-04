using MicroCommerce.ApiService.Domain.Entities;

namespace MicroCommerce.ApiService.UseCases.Categories;

public record CategoryViewModel
{
    public CategoryViewModel(Category domain)
    {
        Id = domain.Id;
        Name = domain.Name;
    }
    
    public CategoryViewModel()
    {
    }
    
    public string Id { get; set; }
    
    public string Name { get; set; }
}