using System.ComponentModel.DataAnnotations;

namespace MicroCommerce.Identity.STS.Identity.ViewModels.Manage
{
    public class SetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }

        public string StatusMessage { get; set; }
    }
}








