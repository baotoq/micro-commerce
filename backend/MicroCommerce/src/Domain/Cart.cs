using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Cart : DateEntity
{
    [MaxLength(100)]
    public string Id { get; set; } = "";
}