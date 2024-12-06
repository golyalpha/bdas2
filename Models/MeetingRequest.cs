using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class MeetingRequest : Request
{
    public required bool VideoCallReady { get; set; }

}
