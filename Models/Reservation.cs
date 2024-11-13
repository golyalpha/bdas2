namespace WebApp.Models;

public class Reservation
{
    public int Id { get; set; }
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
    public required Room Room { get; set; }
    public required Request Request { get; set; }
    public required Organiser Organiser { get; set; }
}
