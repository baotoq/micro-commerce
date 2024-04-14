using System.ComponentModel.DataAnnotations;
using MicroCommerce.ApiService.Domain.Common;

namespace MicroCommerce.ApiService.Domain.Entities;

public class Seller : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";
    [MaxLength(100)]
    public string PhoneNumber { get; set; } = "";
}
