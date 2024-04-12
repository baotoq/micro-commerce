using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class DeliveryAddress : EntityBase
{
    [MaxLength(500)] public string AddressLine { get; set; } = "";
    [MaxLength(500)] public string City { get; set; } = "";

    [MaxLength(500)] public string RecipientName { get; set; } = "";
    [MaxLength(500)] public string RecipientPhoneNumber { get; set; } = "";
    
    [MaxLength(500)] public string DeliveryInstruction { get; set; } = "";
}