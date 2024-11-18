namespace WebApp.Models;

public class PresentationRoom : Room
{
    public bool PodiumSize { get; set; }

    public static PresentationRoom GetPresentationRoom(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<PresentationRoom> ListPresentationRooms()
    {
        throw new NotImplementedException();
    }
}
