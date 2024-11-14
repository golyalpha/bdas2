namespace WebApp.Models;

public class MeetingRequest : Request
{
    public required bool VideoCallReady { get; set; }
}
