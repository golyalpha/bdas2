using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class LocationViewModel
{
    [Required]
    public Location Location { get; set; }
    [Required]
    public int CityId { get; set; }

    public List<City> Cities { get; set; } = City.ListCities();
    
}
