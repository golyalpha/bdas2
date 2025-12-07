using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class MeetingRoom : Room
{
    public bool VideoCallReady { get; set; }

    public void Persist()
    {
        using (var conn = DBManager.GetConnection())
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "rooms_pkg.persist_room";

            cmd.Parameters.Add("p_id_room", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_name", Name);
            cmd.Parameters.Add("p_capacity", Capacity);
            cmd.Parameters.Add("p_type", "MEETING_ROOM");
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);
            cmd.Parameters.Add("p_vc_ready", VideoCallReady ? "Y" : "N");

            cmd.ExecuteNonQuery();
        }
    }
}