namespace WebApp.Models;

public class Request
{
    public int Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public required Organiser Organiser { get; set; }
}
