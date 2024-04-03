using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class Category : EntityBase
{
    [MaxLength(500)]
    public string Name { get; set; } = "";

    public string ImageUrl { get; set; } = "";
}