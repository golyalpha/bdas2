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

    [Display(Name = "End Time")]
    public DateTime End { get; set; }
    
    [Display(Name = "Reservation Length")]
    public DateTime Length { get; set; }

    [Display(Name = "Minimum Capacity")]
    public int MinimumCapacity { get; set; }

    [Display(Name = "Room Request Type")]
    public String Type { get; set; }

    public Location Location { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.edit_request";
            cmd.BindByName = true;

            // Mapování na parametr names v balíčku
            cmd.Parameters.Add("p_id_room_request", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_min_capacity", MinimumCapacity);
            cmd.Parameters.Add("p_type", Type);
            cmd.Parameters.Add("p_reservation_start", Start);
            cmd.Parameters.Add("p_reservation_end", End);
            cmd.Parameters.Add("p_reservation_length", Length);
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);

            // Subtypové parametry (volitelné podle typu)
            if (Type == "PRESENTATION_RREQUEST")
            {
                var pr = (PresentationRequest)this;
                cmd.Parameters.Add("p_podium_size", pr.PodiumSize);
                cmd.Parameters.Add("p_vc_ready", DBNull.Value);
            }
            else if (Type == "MEETING_RREQUEST")
            {
                var mr = (MeetingRequest)this;
                cmd.Parameters.Add("p_vc_ready", mr.VideoCallReady ? "Y" : "N");
                cmd.Parameters.Add("p_podium_size", DBNull.Value);
            }
            else
            {
                // čistý ROOM_REQUEST bez subtype
                cmd.Parameters.Add("p_vc_ready", DBNull.Value);
                cmd.Parameters.Add("p_podium_size", DBNull.Value);
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
            cmd.CommandText = @"SELECT id_organizer, id_location, id_room_request, min_capacity, reservation_start, reservation_length, reservation_end, ""type"", vc_ready, podium_size FROM ROOM_REQUESTS_V WHERE id_room_request = :id";
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

                    // P�ekontrolujeme typ po�adavku
                    if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = reader.GetDateTime(6),
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
                            Length = reader.GetDateTime(6),
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

    public static List<RoomRequest> GetRequestsByOrganiserId(int organiserId)
    {
        List<RoomRequest> list = new List<RoomRequest>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ROOM_REQUESTS_V WHERE id_organizer = :id";
            cmd.Parameters.Add("id", organiserId);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    RoomRequest? request = null;

                    // Překontrolujeme typ požadavku
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
                            VideoCallReady = reader.GetString(8) == "Y",
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
}