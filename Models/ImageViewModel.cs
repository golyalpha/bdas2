using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

/// <summary>
/// ViewModel pro zobrazení obrázku s daty z více tabulek (rooms, locations, organizers)
/// </summary>
public class ImageViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Název souboru")]
    public string FileName { get; set; }
    
    [Display(Name = "Pøípona")]
    public string FileSuffix { get; set; }
    
    [Display(Name = "Datum nahrání")]
    public DateTime CreatedAt { get; set; }
    
    // Data z tabulky rooms
    public int RoomId { get; set; }
    
    [Display(Name = "Místnost")]
    public string RoomName { get; set; }
    
    // Data z tabulky locations
    [Display(Name = "Budova")]
    public string LocationName { get; set; }
    
    // Data z tabulky cities (pøes locations)
    [Display(Name = "Mìsto")]
    public string CityName { get; set; }
    
    // Data z tabulky organizers
    [Display(Name = "Nahrál")]
    public string OrganizerName { get; set; }
}