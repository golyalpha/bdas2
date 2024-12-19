using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class MeetingRequest : RoomRequest
{
    public required bool VideoCallReady { get; set; }

}
