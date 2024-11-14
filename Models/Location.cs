namespace WebApp.Models;

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateTime AvailabilityStart  { get; set; }
    public required DateTime AvailabilityEnd { get; set; }
}
