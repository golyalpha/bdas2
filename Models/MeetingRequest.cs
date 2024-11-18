namespace WebApp.Models;

public class MeetingRequest : Request
{
    public required bool VideoCallReady { get; set; }

    public static MeetingRequest GetMeetingRequest(int Id)
    {
        throw new NotImplementedException();
    }

    public static List<MeetingRequest> ListMeetingRequests()
    {
        throw new NotImplementedException();
    }
}
