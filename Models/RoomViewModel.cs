namespace WebApp.Models
    {
        public class RoomViewModel
        {
            public Room? Room { get; set; }

            public bool? VideoCallReady { get; set; } 
            public int? PodiumSize { get; set; }

            public int LocationId { get; set; }
            public int OrganiserId { get; set; }

            public List<Location> Locations { get; set; }
            public List<Organiser> Organisers { get; set; }
    }
}
