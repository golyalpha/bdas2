using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Jméno je povinné")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Jméno musí mít 2-100 znaků")]
    [Display(Name = "Jméno")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email je povinné")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    [StringLength(100, ErrorMessage = "Email může mít maximálně 100 znaků")]
    [Display(Name = "Email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Heslo je povinné")]
    [PasswordValidation]
    [DataType(DataType.Password)]
    [Display(Name = "Heslo")]
    public required string Password { get; set; }
}
