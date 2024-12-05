using System.ComponentModel.DataAnnotations;

using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Request
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Start Time is required")]
    [Display(Name = "Start Time")]
    public DateTime Start { get; set; }

    [Required(ErrorMessage = "End Time is required")]
    [Display(Name = "End Time")]
    public DateTime End { get; set; }
    public TimeSpan Length { get; set; }
    public int MinimumCapacity { get; set; }
    public String Type { get; set; }
    public Location Location { get; set; }
    public required Organiser Organiser { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    // Mock metoda pro naètení konkrétní žádosti (simulace)
    public static Request GetRequest(int id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, id_location, id_room_request, min_capacity, reservation_start, reservation_length, reservation_end, type, vc_ready, podium_size FROM ROOM_REQUEST_V WHERE id_room_request = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                Request? request = null;
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                if (reader.GetString(5) == "PRESENTATION_RREQUEST")
                {
                    request = new PresentationRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = reader.GetTimeSpan(5),
                        End = reader.GetDateTime(6),
                        Type = reader.GetString(7),
                        PodiumSize = reader.GetInt32(9),
                        Location = Location.GetLocation(reader.GetInt32(1)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                    };
                }
                if (reader.GetString(5) == "MEETING_RREQUEST")
                {
                    request = new MeetingRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = reader.GetTimeSpan(5),
                        End = reader.GetDateTime(6),
                        Type = reader.GetString(7),
                        VideoCallReady = reader.GetBoolean(8),
                        Location = Location.GetLocation(reader.GetInt32(1)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                    };
                }
                if (request == null)
                {
                    throw new InvalidDataException();
                }
                reader.Close();
                conn.Close();
                return request;
            }
        }
    }

    public static List<Request> ListRequests()
    {
        List<Request> list = new List<Request>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, id_location, id_room_request, min_capacity, reservation_start, reservation_length, reservation_end, type, vc_ready, podium_size FROM ROOM_REQUEST_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Request? request = null;
                    reader.Read();
                    if (reader.GetString(5) == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            Length = reader.GetTimeSpan(5),
                            End = reader.GetDateTime(6),
                            Type = reader.GetString(7),
                            PodiumSize = reader.GetInt32(9),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    if (reader.GetString(5) == "MEETING_RREQUEST")
                    {
                        request = new MeetingRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            Length = reader.GetTimeSpan(5),
                            End = reader.GetDateTime(6),
                            Type = reader.GetString(7),
                            VideoCallReady = reader.GetBoolean(8),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    if (request == null)
                    {
                        continue;
                    }
                    list.Add(request);
                }
            }
        }
        return list;
    }
}