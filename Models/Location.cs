namespace WebApp.Models;

public class Location
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required TimeOnly AvailabilityStart  { get; set; }
    public required TimeOnly AvailabilityEnd { get; set; }

    public static Location GetLocation(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<Location> ListLocations()
    {
        throw new NotImplementedException();
    }
}
