using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace aspnetcoreapp.Pages;

    public class ReservationsModel : PageModel
    {
        // Property to hold the list of reservations to be displayed on the page
        public List<Reservation> Reservations { get; set; }

        public void OnGet()
        {
            // Simulate data retrieval (replace this with database code)
            Reservations = new List<Reservation>
            {
                new Reservation { ID = 1, StartTime = DateTime.Parse("2023-11-01 10:00"), EndTime = DateTime.Parse("2023-11-01 12:00"), RoomID = 101, OrganizerID = 5, RoomRequestID = 23 },
                new Reservation { ID = 2, StartTime = DateTime.Parse("2023-11-02 09:00"), EndTime = DateTime.Parse("2023-11-02 11:00"), RoomID = 102, OrganizerID = 6, RoomRequestID = 24 },
                new Reservation { ID = 3, StartTime = DateTime.Parse("2023-11-03 14:00"), EndTime = DateTime.Parse("2023-11-03 16:00"), RoomID = 103, OrganizerID = 7, RoomRequestID = 25 }
            };
        }
    }

    // Reservation model representing a reservation record
    public class Reservation
    {
        public int ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RoomID { get; set; }
        public int OrganizerID { get; set; }
        public int RoomRequestID { get; set; }
    }

