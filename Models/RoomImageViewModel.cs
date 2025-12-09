using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class RoomImageViewModel
{
    [Required]
    public int IdRoom { get; set; }

    [Required]
    public int IdOrganizer { get; set; }

    [Required]
    public int IdLocation { get; set; }

    [Required(ErrorMessage = "Musíte vybrat obrázek")]
    [Display(Name = "Obrázek")]
    public IFormFile ImageFile { get; set; }
}