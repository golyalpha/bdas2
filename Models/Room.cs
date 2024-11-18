namespace WebApp.Models;

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required uint Capacity { get; set; }
    public required string Type { get; set; }
    public required Location Location { get; set; }

    public static Room GetRoom(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<Room> ListRooms()
    {
        throw new NotImplementedException();
    }
}
