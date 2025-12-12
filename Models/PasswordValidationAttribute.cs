using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebApp.Models;

/// <summary>
/// Validaèní atribut pro kontrolu složitosti hesla.
/// Požadavky:
/// - Minimálnì 6 znakù
/// - Alespoò 1 èíslice (0-9)
/// - Alespoò 1 velké písmeno (A-Z)
/// - Alespoò 1 malé písmeno (a-z)
/// - Alespoò 1 speciální znak (!@#$%^&*()_+-=[]{}|;:,.<>?)
/// </summary>
public class PasswordValidationAttribute : ValidationAttribute
{
    private const int MinLength = 6;

    public PasswordValidationAttribute()
    {
        ErrorMessage = "Heslo musí obsahovat minimálnì {0} znakù, alespoò jednu èíslici, jedno velké písmeno, jedno malé písmeno a jeden speciální znak";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Heslo je povinné");
        }

        string password = value.ToString()!;

        // Kontrola minimální délky
        if (password.Length < MinLength)
        {
            return new ValidationResult($"Heslo musí mít minimálnì {MinLength} znakù");
        }

        // Kontrola èíslice
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return new ValidationResult("Heslo musí obsahovat alespoò jednu èíslici (0-9)");
        }

        // Kontrola velkého písmene
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return new ValidationResult("Heslo musí obsahovat alespoò jedno velké písmeno (A-Z)");
        }

        // Kontrola malého písmene
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return new ValidationResult("Heslo musí obsahovat alespoò jedno malé písmeno (a-z)");
        }

        // Kontrola speciálního znaku
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
        {
            return new ValidationResult("Heslo musí obsahovat alespoò jeden speciální znak (!@#$%^&*()_+-=[]{}|;:,.<>?)");
        }

        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessage!, MinLength);
    }
}