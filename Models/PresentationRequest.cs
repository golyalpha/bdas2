using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class PresentationRequest : RoomRequest
{
    public required int PodiumSize { get; set; }
}

