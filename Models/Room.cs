using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int Capacity { get; set; }
    public required string Type { get; set; }
    public required Location Location { get; set; }

    public static Room GetRoom(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_room, id_location, name, capacity, type, vc_ready, podium_size FROM ROOMS_V WHERE id_room = :1";
            cmd.Parameters.Add(Id);
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
                        PodiumSize = reader.GetBoolean(6),
                        Location = Location.GetLocation(reader.GetInt32(0)),
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

    public static List<Room> ListRooms()
    {
        List<Room> list = new List<Room>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_room, id_location, name, capacity, 'type', vc_ready, podium_size FROM ROOMS_V"; 
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Room? room = null;
                    reader.Read();
                    if (reader.GetString(4) == "PRESENTATION_ROOM")
                    {
                        room = new PresentationRoom
                        {
                            Id = reader.GetInt32(1),
                            Name = reader.GetString(2),
                            Capacity = reader.GetInt32(3),
                            Type = reader.GetString(4),
                            PodiumSize = reader.GetBoolean(6),
                            Location = Location.GetLocation(reader.GetInt32(0)),
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
                        };
                    }
                    if (room == null)
                    {
                        continue;
                    }
                    list.Add(room);
                }
            }
        }
        return list;
    }
}
