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

    // OPRAVENO: GetRequest() - použití nového VIEW
    public static RoomRequest GetRequest(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using (OracleCommand cmd = conn.CreateCommand())
            {
                // Používáme nový VIEW s JOINy
                cmd.CommandText = @"
                    SELECT 
                        id_room_request, min_capacity, reservation_start, reservation_end, 
                        reservation_length, ""type"", vc_ready, podium_size,
                        id_location, location_name, availability_start, availability_end,
                        id_city, city_name, id_country, country_name,
                        id_organizer, organizer_name, organizer_email, id_role, role_name
                    FROM ROOM_REQUESTS_WITH_DETAILS_V 
                    WHERE id_room_request = :id";
                cmd.Parameters.Add("id", Id);
                cmd.CommandType = System.Data.CommandType.Text;
                
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new KeyNotFoundException();
                    }
                    
                    TimeSpan length = TimeSpan.Zero;
                    if (!reader.IsDBNull(4))
                    {
                        OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(4);
                        length = intervalValue.Value;
                    }
                    
                    // Vytvoříme objekty bez dalších DB dotazů
                    var country = new Country
                    {
                        Id = reader.GetInt32(14),
                        Name = reader.GetString(15)
                    };
                    
                    var city = new City
                    {
                        Id = reader.GetInt32(12),
                        Name = reader.GetString(13),
                        Country = country
                    };
                    
                    var role = new Role
                    {
                        Id = reader.GetInt32(19),
                        Name = reader.GetString(20)
                    };
                    
                    var organiser = new Organiser
                    {
                        Id = reader.GetInt32(16),
                        Name = reader.GetString(17),
                        Email = reader.GetString(18),
                        Role = role
                    };
                    
                    var location = new Location
                    {
                        Id = reader.GetInt32(8),
                        Name = reader.GetString(9),
                        AvailabilityStart = TimeOnly.FromDateTime(reader.GetDateTime(10)),
                        AvailabilityEnd = TimeOnly.FromDateTime(reader.GetDateTime(11)),
                        City = city,
                        Organiser = organiser
                    };
                    
                    string type = reader.GetString(5);
                    RoomRequest? request = null;
                    
                    if (type == "PRESENTATION_RREQUEST")
                    {
                        request = new PresentationRequest
                        {
                            Id = reader.GetInt32(0),
                            MinimumCapacity = reader.GetInt32(1),
                            Start = reader.GetDateTime(2),
                            End = reader.GetDateTime(3),
                            Length = length,
                            Type = type,
                            PodiumSize = reader.GetInt32(7),
                            Location = location,
                            Organiser = organiser
                        };
                    }
                    else if (type == "MEETING_RREQUEST")
                    {
                        request = new MeetingRequest
                        {
                            Id = reader.GetInt32(0),
                            MinimumCapacity = reader.GetInt32(1),
                            Start = reader.GetDateTime(2),
                            End = reader.GetDateTime(3),
                            Length = length,
                            Type = type,
                            VideoCallReady = reader.GetString(6) == "Y",
                            Location = location,
                            Organiser = organiser
                        };
                    }
                    
                    if (request == null)
                    {
                        throw new InvalidDataException();
                    }
                    
                    return request;
                }
            }
        }
    }

    //OPRAVENO: ListRequests() - použití nového VIEW
    public static List<RoomRequest> ListRequests()
    {
        List<RoomRequest> list = new List<RoomRequest>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using (OracleCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT 
                        id_room_request, min_capacity, reservation_start, reservation_end, 
                        reservation_length, ""type"", vc_ready, podium_size,
                        id_location, location_name, availability_start, availability_end,
                        id_city, city_name, id_country, country_name,
                        id_organizer, organizer_name, organizer_email, id_role, role_name
                    FROM ROOM_REQUESTS_WITH_DETAILS_V";
                cmd.CommandType = System.Data.CommandType.Text;
                
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TimeSpan length = TimeSpan.Zero;
                        if (!reader.IsDBNull(4))
                        {
                            OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(4);
                            length = intervalValue.Value;
                        }
                        
                        //Vytvoříme objekty bez dalších DB dotazů
                        var country = new Country
                        {
                            Id = reader.GetInt32(14),
                            Name = reader.GetString(15)
                        };
                        
                        var city = new City
                        {
                            Id = reader.GetInt32(12),
                            Name = reader.GetString(13),
                            Country = country
                        };
                        
                        var role = new Role
                        {
                            Id = reader.GetInt32(19),
                            Name = reader.GetString(20)
                        };
                        
                        var organiser = new Organiser
                        {
                            Id = reader.GetInt32(16),
                            Name = reader.GetString(17),
                            Email = reader.GetString(18),
                            Role = role
                        };
                        
                        var location = new Location
                        {
                            Id = reader.GetInt32(8),
                            Name = reader.GetString(9),
                            AvailabilityStart = TimeOnly.FromDateTime(reader.GetDateTime(10)),
                            AvailabilityEnd = TimeOnly.FromDateTime(reader.GetDateTime(11)),
                            City = city,
                            Organiser = organiser
                        };
                        
                        string type = reader.GetString(5);
                        RoomRequest? request = null;
                        
                        if (type == "PRESENTATION_RREQUEST")
                        {
                            request = new PresentationRequest
                            {
                                Id = reader.GetInt32(0),
                                MinimumCapacity = reader.GetInt32(1),
                                Start = reader.GetDateTime(2),
                                End = reader.GetDateTime(3),
                                Length = length,
                                Type = type,
                                PodiumSize = reader.GetInt32(7),
                                Location = location,
                                Organiser = organiser
                            };
                        }
                        else if (type == "MEETING_RREQUEST")
                        {
                            request = new MeetingRequest
                            {
                                Id = reader.GetInt32(0),
                                MinimumCapacity = reader.GetInt32(1),
                                Start = reader.GetDateTime(2),
                                End = reader.GetDateTime(3),
                                Length = length,
                                Type = type,
                                VideoCallReady = !reader.IsDBNull(6) && reader.GetString(6) == "Y",
                                Location = location,
                                Organiser = organiser
                            };
                        }
                        
                        if (request != null)
                        {
                            list.Add(request);
                        }
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
            using (OracleCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "requests_pkg.delete_request";
                cmd.Parameters.Add("p_id_request", Id);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // OPRAVENO: GetRequestsByOrganiserId() - použití nového VIEW
    public static List<RoomRequest> GetRequestsByOrganiserId(int organiserId)
    {
        List<RoomRequest> list = new List<RoomRequest>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using (OracleCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT 
                        id_room_request, min_capacity, reservation_start, reservation_end, 
                        reservation_length, ""type"", vc_ready, podium_size,
                        id_location, location_name, availability_start, availability_end,
                        id_city, city_name, id_country, country_name,
                        id_organizer, organizer_name, organizer_email, id_role, role_name
                    FROM ROOM_REQUESTS_WITH_DETAILS_V
                    WHERE id_organizer = :id";
                cmd.Parameters.Add("id", organiserId);
                cmd.CommandType = System.Data.CommandType.Text;
                
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TimeSpan length = TimeSpan.Zero;
                        if (!reader.IsDBNull(4))
                        {
                            OracleIntervalDS intervalValue = reader.GetOracleIntervalDS(4);
                            length = intervalValue.Value;
                        }
                        
                        // Vytvoříme objekty bez dalších DB dotazů (stejný kód jako výše)
                        var country = new Country
                        {
                            Id = reader.GetInt32(14),
                            Name = reader.GetString(15)
                        };
                        
                        var city = new City
                        {
                            Id = reader.GetInt32(12),
                            Name = reader.GetString(13),
                            Country = country
                        };
                        
                        var role = new Role
                        {
                            Id = reader.GetInt32(19),
                            Name = reader.GetString(20)
                        };
                        
                        var organiser = new Organiser
                        {
                            Id = reader.GetInt32(16),
                            Name = reader.GetString(17),
                            Email = reader.GetString(18),
                            Role = role
                        };
                        
                        var location = new Location
                        {
                            Id = reader.GetInt32(8),
                            Name = reader.GetString(9),
                            AvailabilityStart = TimeOnly.FromDateTime(reader.GetDateTime(10)),
                            AvailabilityEnd = TimeOnly.FromDateTime(reader.GetDateTime(11)),
                            City = city,
                            Organiser = organiser
                        };
                        
                        string type = reader.GetString(5);
                        RoomRequest? request = null;
                        
                        if (type == "PRESENTATION_RREQUEST")
                        {
                            request = new PresentationRequest
                            {
                                Id = reader.GetInt32(0),
                                MinimumCapacity = reader.GetInt32(1),
                                Start = reader.GetDateTime(2),
                                End = reader.GetDateTime(3),
                                Length = length,
                                Type = type,
                                PodiumSize = reader.GetInt32(7),
                                Location = location,
                                Organiser = organiser
                            };
                        }
                        else if (type == "MEETING_RREQUEST")
                        {
                            request = new MeetingRequest
                            {
                                Id = reader.GetInt32(0),
                                MinimumCapacity = reader.GetInt32(1),
                                Start = reader.GetDateTime(2),
                                End = reader.GetDateTime(3),
                                Length = length,
                                Type = type,
                                VideoCallReady = reader.GetString(6) == "Y",
                                Location = location,
                                Organiser = organiser
                            };
                        }
                        
                        if (request != null)
                        {
                            list.Add(request);
                        }
                    }
                }
            }
        }
        return list;
    }
}