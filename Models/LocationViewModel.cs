using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class LocationViewModel
{
    public Location? Location { get; set; }

    [Required]
    [Display(Name = "City")]
    public int CityId { get; set; }

    public List<City> Cities { get; set; } = City.ListCities();
    
}
