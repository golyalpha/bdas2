namespace WebApp.Models;

public class MeetingRoom : Room
{
    public bool VideoCallReady { get; set; }

    public static MeetingRoom GetMeetingRequest(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<MeetingRoom> ListMeetingRooms()
    {
        throw new NotImplementedException();
    }
}
