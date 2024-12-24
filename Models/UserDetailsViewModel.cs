using Microsoft.AspNetCore.Mvc;

namespace WebApp.Models
{
    public class UserDetailsViewModel
    {
    public Organiser User { get; set; }
    public List<Reservation> Reservations { get; set; }
    public List<RoomRequest> Requests { get; set; }
    }
}
