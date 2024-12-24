using System.ComponentModel.DataAnnotations;
using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class MeetingRequest : RoomRequest
{
    [Display(Name = "Video Call Required")]
    public required bool VideoCallReady { get; set; }

}
