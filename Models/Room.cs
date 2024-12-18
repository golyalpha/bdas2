using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;
namespace WebApp.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
    
    [Required]
    public int Capacity { get; set; }
    [Required]
    public string Type { get; set; }
    
    [Required]
    
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
            cmd.CommandText = "edit_room";
            cmd.Parameters.Add("id", Id);
            cmd.Parameters.Add("name", Name);
            cmd.Parameters.Add("capacity", Capacity);
            cmd.Parameters.Add("type", Type);
            cmd.Parameters.Add("location_id", Location.Id);
            if (Type == "PRESENTATION_ROOM")
            {
                PresentationRoom room = (PresentationRoom)this;
                cmd.Parameters.Add("podium_size", room.PodiumSize);
            }
            if (Type == "MEETING_ROOM")
            {
                MeetingRoom room = (MeetingRoom)this;
                cmd.Parameters.Add("vc_ready", room.VideoCallReady);
            }
            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
            {
                throw new ApplicationException("Persisting entity failed, no rows were updated.");
            }
        }
    }
    public static Room GetRoom(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_room, id_location, name, capacity, type, vc_ready, podium_size, id_organizer FROM ROOMS_V WHERE id_room = :id";
            cmd.Parameters.Add("id", Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                Room? room = null;
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                if (reader.GetString(4) == "PRESENTATION_ROOM")
                {
                    room = new PresentationRoom
                    {
                        Id = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        Capacity = reader.GetInt32(3),
                        Type = reader.GetString(4),
                        PodiumSize = reader.GetInt32(6),
                        Location = Location.GetLocation(reader.GetInt32(0)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(7))
                    };
                }
                if (reader.GetString(4) == "MEETING_ROOM")
                {
                    room = new MeetingRoom
                    {
                        Id = reader.GetInt32(1),
                        Name = reader.GetString(2),
                        Capacity = reader.GetInt32(3),
                        Type = reader.GetString(4),
                        VideoCallReady = reader.GetBoolean(5),
                        Location = Location.GetLocation(reader.GetInt32(0)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(7))
                    };
                }
                if (room == null)
                {
                    throw new InvalidDataException();
                }
                reader.Close();
                conn.Close();
                return room;
            }
        }
    }

    public static List<object> ListRooms()
    {
        List<object> list = new List<object>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_room, id_location, name, capacity, type, vc_ready, podium_size, id_organizer FROM ROOMS_V"; 
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    object? room = null;
                    //reader.Read();
                    if (reader.GetString(4) == "PRESENTATION_ROOM")
                    {
                        room = new PresentationRoom
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            Type = reader.GetString(4),
                            PodiumSize = reader.GetInt32(6),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(7))
                        };
                    }
                    if (reader.GetString(4) == "MEETING_ROOM")
                    {
                        room = new MeetingRoom
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            Type = reader.GetString(4),
                            VideoCallReady = reader.GetBoolean(5),
                            Location = Location.GetLocation(reader.GetInt32(1)),
                            Organiser = Organiser.GetOrganiser(reader.GetInt32(7))
                        };
                    }
                    if (room == null)
                    {
                        throw new InvalidDataException();
                    }
                    list.Add(room);
                }
            }
        }
        return list;
    }

    public void Create()
    {
        throw new Exception("Not implemented");
    }


    public void Update()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.update_room";

            cmd.Parameters.Add("p_id_room", Id);    
            cmd.Parameters.Add("p_name", Name);
            cmd.Parameters.Add("p_capacity", Capacity);
            cmd.Parameters.Add("p_type", Type);
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);


            cmd.ExecuteNonQuery();
        }
    }

    public static void Delete(int roomId)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.delete_room";
            cmd.Parameters.Add("p_id_room", roomId);

            cmd.ExecuteNonQuery();
        }
    }
}
