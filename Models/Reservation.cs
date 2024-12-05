using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class Reservation
{
    public int Id { get; set; }
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
    public required Room Room { get; set; }
    public required Request Request { get; set; }
    public required Organiser Organiser { get; set; }

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
        throw new NotImplementedException();
    }
}
