using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class MeetingRoom : Room
{
    public bool VideoCallReady { get; set; }

    public void Create()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.create_room";

            // Pøidání parametrù
            cmd.Parameters.Add("p_name", Name);
            cmd.Parameters.Add("p_capacity", Capacity);
            cmd.Parameters.Add("p_type", "MEETING_ROOM");
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);
            cmd.Parameters.Add("p_vc_ready", VideoCallReady ? "Y" : "N"); // Pøevod bool na CHAR
            cmd.Parameters.Add("p_podium_size", DBNull.Value); // Povinný parametr jako NULL

            cmd.ExecuteNonQuery();
        }
    }
}