using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    [Display(Name = "Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Nové heslo je povinné")]
    [PasswordValidation]
    [DataType(DataType.Password)]
    [Display(Name = "Nové heslo")]
    public required string NewPassword { get; set; }

    [Required(ErrorMessage = "Potvrzení hesla je povinné")]
    [DataType(DataType.Password)]
    [Display(Name = "Potvrdit nové heslo")]
    [Compare("NewPassword", ErrorMessage = "Hesla se neshodují")]
    public required string ConfirmPassword { get; set; }
}