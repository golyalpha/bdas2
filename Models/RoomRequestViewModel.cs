using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public class RoomRequestViewModel
{
    public RoomRequest? Request { get; set; }
    public MeetingRequest? MeetingRequest { get; set; }
    public PresentationRequest? PresentationRequest { get; set; }

    [Required(ErrorMessage = "The Location field is required.")]
    [Display(Name = "Location")]
    public int LocationId { get; set; }

    public List<Location> Locations { get; set; } = Location.ListLocations();
    
}
