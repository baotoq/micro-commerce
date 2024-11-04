using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Category : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";

    public string ImageUrl { get; set; } = "";
}