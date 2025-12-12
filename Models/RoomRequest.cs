using System;
using System.ComponentModel.DataAnnotations;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class RoomRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Start Time is required")]
    [Display(Name = "Začátek rezervace")]
    public DateTime Start { get; set; }

    [Display(Name = "Konec rezervace")]
    public DateTime End { get; set; }
    
    [Display(Name = "Délka rezervace")]
    public TimeSpan Length { get; set; }

    [Display(Name = "Minimální kapacita")]
    public int MinimumCapacity { get; set; }

    [Display(Name = "Požadavek na typ místnosti")]
    public String Type { get; set; }

    public Location Location { get; set; }

    public Organiser Organiser { get; set; }


   
    /// <summary>
    /// Formátované zobrazení délky jako "HH:MMh" (např. "2:30h")
    /// </summary>
    public string GetFormattedLength()
    {
        int totalHours = (int)Length.TotalHours;
        int minutes = Length.Minutes;
        return $"{totalHours}:{minutes:00}h";
    }

    /// <summary>
    /// Kontrola zda pro tuto žádost existuje rezervace
    /// </summary>
    public bool HasReservation()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT COUNT(*) FROM reservations WHERE id_room_request = :id";
            cmd.Parameters.Add("id", OracleDbType.Int32).Value = Id;
            
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

    /// <summary>
    /// Výpočet délky pomocí DB funkce - vrací TimeSpan
    /// </summary>
    public static TimeSpan CalculateLength(DateTime start, DateTime end)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT requests_pkg.calculate_length(:p_start, :p_end) FROM DUAL";
            cmd.Parameters.Add("p_start", OracleDbType.Date).Value = start;
            cmd.Parameters.Add("p_end", OracleDbType.Date).Value = end;
            
            OracleIntervalDS interval = (OracleIntervalDS)cmd.ExecuteScalar();
            return interval.Value;
        }
    }

    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "requests_pkg.persist_request";
            cmd.BindByName = true;

            cmd.Parameters.Add("p_id_room_request", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_min_capacity", MinimumCapacity);
            cmd.Parameters.Add("p_type", Type);
            cmd.Parameters.Add("p_reservation_start", OracleDbType.Date).Value = Start;
            cmd.Parameters.Add("p_reservation_end", OracleDbType.Date).Value = End;
            
            //  OPRAVA: Správná konverze TimeSpan na OracleIntervalDS
            if (Length != TimeSpan.Zero)
            {
                cmd.Parameters.Add("p_reservation_length", OracleDbType.IntervalDS).Value = 
                    new OracleIntervalDS(Length.Days, Length.Hours, Length.Minutes, Length.Seconds, 0);
            }
            else
            {
                cmd.Parameters.Add("p_reservation_length", OracleDbType.IntervalDS).Value = DBNull.Value;
            }
            
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);

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
                
                TimeSpan length = TimeSpan.Zero;
                if (!reader.IsDBNull(5))
                {
                    OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(5);
                    length = intervalValue.Value;
                }
                
                if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                {
                    request = new PresentationRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = length,
                        End = reader.GetDateTime(6),
                        Type = reader.GetString(7),
                        PodiumSize = reader.GetInt32(9),
                        Location = Location.GetLocation(reader.GetInt32(1)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                    };
                }
                else if (reader.GetString(7) == "MEETING_RREQUEST")
                {
                    request = new MeetingRequest
                    {
                        Id = reader.GetInt32(2),
                        MinimumCapacity = reader.GetInt32(3),
                        Start = reader.GetDateTime(4),
                        Length = length,
                        End = reader.GetDateTime(6),
                        Type = reader.GetString(7),
                        VideoCallReady = reader.GetString(8) == "Y",
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
                    
                    // ZMĚNA: Načtení INTERVAL jako TimeSpan
                    TimeSpan length = TimeSpan.Zero;
                    if (!reader.IsDBNull(6))
                    {
                        OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(6);
                        length = intervalValue.Value;
                    }

                    if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = length,
                            Type = reader.GetString(7),
                            PodiumSize = reader.GetInt32(9),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    else if (reader.GetString(7) == "MEETING_RREQUEST")
                    {
                        request = new MeetingRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = length,
                            Type = reader.GetString(7),
                            VideoCallReady = !reader.IsDBNull(8) && reader.GetString(8) == "Y",
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
            cmd.CommandText = "requests_pkg.delete_request";
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
                    
                    TimeSpan length = TimeSpan.Zero;
                    if (!reader.IsDBNull(6))
                    {
                        OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(6);
                        length = intervalValue.Value;
                    }

                    if (reader.GetString(7) == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = length,
                            Type = reader.GetString(7),
                            PodiumSize = reader.GetInt32(9),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(0))
                        };
                    }
                    else if (reader.GetString(7) == "MEETING_RREQUEST")
                    {
                        request = new MeetingRequest
                        {
                            Id = reader.GetInt32(2),
                            MinimumCapacity = reader.GetInt32(3),
                            Start = reader.GetDateTime(4),
                            End = reader.GetDateTime(5),
                            Length = length,
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