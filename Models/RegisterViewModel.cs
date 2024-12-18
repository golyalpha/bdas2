using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class RegisterViewModel
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
    
}
