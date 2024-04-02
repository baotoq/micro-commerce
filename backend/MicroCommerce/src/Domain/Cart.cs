using System.ComponentModel.DataAnnotations;

namespace Domain;

public class Cart : DateTimeEntity
{
    [MaxLength(100)]
    public string Id { get; set; } = "";
}