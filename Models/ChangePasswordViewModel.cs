using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Staré heslo je povinné")]
    [DataType(DataType.Password)]
    [Display(Name = "Staré heslo")]
    public required string OldPassword { get; set; }

    [Required(ErrorMessage = "Zadejte nové heslo")]
    [PasswordValidation]
    [DataType(DataType.Password)]
    [Display(Name = "Nové heslo")]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "Zadejte heslo znovu")]
    [DataType(DataType.Password)]
    [Display(Name = "Potvrdit nové heslo")]
    [Compare("NewPassword", ErrorMessage = "Nové heslo a potvrzení se neshodují")]
    public required string ConfirmPassword { get; set; }
}