namespace WebApp.Models;

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required uint Capacity { get; set; }
    public required string Type { get; set; }
    public required Location Location { get; set; }
}
