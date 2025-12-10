using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class ImageViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Název souboru")]
    public string FileName { get; set; }
    
    [Display(Name = "Pøípona")]
    public string FileSuffix { get; set; }
    
    [Display(Name = "Datum nahrání")]
    public DateTime CreatedAt { get; set; }
    
    public int RoomId { get; set; }
    
    [Display(Name = "Místnost")]
    public string RoomName { get; set; }
    
    [Display(Name = "Budova")]
    public string LocationName { get; set; }
    
    [Display(Name = "Mìsto")]
    public string CityName { get; set; }
    
    [Display(Name = "Nahrál")]
    public string OrganizerName { get; set; }
}