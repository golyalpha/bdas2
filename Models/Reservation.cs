using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    [Required]
    public Room Room { get; set; }

    [Required]
    public Request Request { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    public static Reservation GetReservation(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_reservation, start, end, id_room, id_room_request, id_organizer FROM RESERVATIONS_V WHERE id_reservation = :1";
            cmd.Parameters.Add(Id);
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Reservation org = new Reservation
                {
                    Id = reader.GetInt32(0),
                    Start = reader.GetDateTime(1),
                    End = reader.GetDateTime(2),
                    Room = Room.GetRoom(reader.GetInt32(4)),
                    Request = Request.GetRequest(reader.GetInt32(5)),
                    Organiser = Organiser.GetOrganiser(reader.GetInt32(6))
                };
                reader.Close();
                conn.Close();
                return org;
            }
        }
    }

    public static List<Reservation> ListReservations()
    {
        List<Reservation> list = new();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM RESERVATIONS_V";
            cmd.CommandType = System.Data.CommandType.Text;
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Reservation
                    {
                        Id = reader.GetInt32(0),
                        Start = reader.GetDateTime(1),
                        End = reader.GetDateTime(2),
                        Room = Room.GetRoom(reader.GetInt32(3)),
                        Request = Request.GetRequest(reader.GetInt32(4)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(5))
                    });
                }
            }
        }
        return list;
    }
}
