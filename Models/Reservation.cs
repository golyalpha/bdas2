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
    public RoomRequest Request { get; set; }

    [Required]
    public Organiser Organiser { get; set; }

    public static Reservation GetReservation(int Id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_reservation, \"start\", \"end\", id_room, id_room_request, id_organizer FROM RESERVATIONS_V WHERE id_reservation = :id";
            cmd.Parameters.Add("id", Id);
            cmd.CommandType = System.Data.CommandType.Text;

            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    throw new KeyNotFoundException();
                }
                Reservation reservation = new Reservation
                {
                    Id = reader.GetInt32(0),
                    Start = reader.GetDateTime(1),
                    End = reader.GetDateTime(2),
                    Room = Room.GetRoom(reader.GetInt32(3)),
                    Request = (RoomRequest)RoomRequest.GetRequest(reader.GetInt32(4)),
                    Organiser = Organiser.GetOrganiser(reader.GetInt32(5))
                };
                return reservation;
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
                        Request = (RoomRequest)RoomRequest.GetRequest(reader.GetInt32(4)),
                        Organiser = Organiser.GetOrganiser(reader.GetInt32(5))
                    });
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
            cmd.CommandText = "reservations_pkg.delete_reservation";
            cmd.Parameters.Add("p_id_reservation", Id);

            cmd.ExecuteNonQuery();
        }
    }


    public void Persist()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.edit_reservation";
            cmd.Parameters.Add("p_id_reservation", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_start", OracleDbType.Date).Value = Start;
            cmd.Parameters.Add("p_end", OracleDbType.Date).Value = End;
            cmd.Parameters.Add("p_id_room", Room.Id);
            cmd.Parameters.Add("p_id_room_request", Request.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);

            cmd.ExecuteNonQuery();
        }
    }
}
