using System.ComponentModel.DataAnnotations;
using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class PresentationRequest : RoomRequest
{
    [Display(Name = "Minimum Podium Size")]
    public required int PodiumSize { get; set; }
}

