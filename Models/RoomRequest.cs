using System;
using System.ComponentModel.DataAnnotations;

using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class RoomRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Start Time is required")]
    [Display(Name = "Start Time")]
    public DateTime Start { get; set; }

    [Required(ErrorMessage = "End Time is required")]
    [Display(Name = "End Time")]
    public DateTime End { get; set; }
    public DateTime Length { get; set; }
    public int MinimumCapacity { get; set; }
    public String Type { get; set; }
    public Location Location { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "edit_reservation_request";
            cmd.Parameters.Add("id", Id);
            cmd.Parameters.Add("reservation_start", Start);
            cmd.Parameters.Add("reservation_end", End);
            cmd.Parameters.Add("reservation_length", Length);
            cmd.Parameters.Add("minimum_capacity", MinimumCapacity);
            cmd.Parameters.Add("type", Type);
            cmd.Parameters.Add("location_id", Location.Id);
            cmd.Parameters.Add("organiser_id", Organiser.Id);
            if (Type == "PRESENTATION_RREQUEST")
            {
                PresentationRequest request = (PresentationRequest)this;
                cmd.Parameters.Add("podium_size", request.PodiumSize);
            }
            if (Type == "MEETING_RREQUEST")
            {
                MeetingRequest request = (MeetingRequest)this;
                cmd.Parameters.Add("vc_ready", request.VideoCallReady);
            }
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }

    public static RoomRequest GetRequest(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_organizer, id_location, id_room_request, min_capacity, reservation_start, reservation_length, reservation_end, type, vc_ready, podium_size FROM ROOM_REQUESTS_V WHERE id_room_request = :id";
            cmd.Parameters.Add("id", Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                RoomRequest? request = null;

                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                {
                    request = new PresentationRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = reader.GetDateTime(5),
                        End = reader.GetDateTime(6),
                        Type = reader.GetString(7),
                        PodiumSize = reader.GetInt32(9),
                        Location = Location.GetLocation(reader.GetInt32(1)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                    };
                }
                if (reader.GetString(7) == "MEETING_RREQUEST")
                {
                    request = new MeetingRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = reader.GetDateTime(5),
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

    public static List<RoomRequest> ListRequests()
    {
        List<RoomRequest> list = new List<RoomRequest>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ROOM_REQUESTS_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    RoomRequest? request = null;

                    // Pøekontrolujeme typ požadavku
                    if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = reader.GetDateTime(6), // Convert INTERVAL to TimeSpan
                            Type = reader.GetString(7),
                            PodiumSize = reader.GetInt32(9),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    if (reader.GetString(7) == "MEETING_RREQUEST")
                    {
                        request = new MeetingRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = reader.GetDateTime(6), // Convert INTERVAL to TimeSpan
                            Type = reader.GetString(7),
                            VideoCallReady = reader.GetString(8) == "Y", // Convert CHAR(1) to Boolean
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    if (request != null)
                    {
                        list.Add(request);
                    }
                }
            }
        }
        return list;
    }

    public void Delete()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.delete_room_request";
            cmd.Parameters.Add("p_id_request", Id);

            cmd.ExecuteNonQuery();
        }
    }

}