using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class PresentationRoom : Room
{
    public int PodiumSize { get; set; }

    public void Persist()
    {
        using (var conn = DBManager.GetConnection())
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.edit_room";

            cmd.Parameters.Add("p_id_room", Id == 0 ? (object)DBNull.Value : Id);
            cmd.Parameters.Add("p_name", Name);
            cmd.Parameters.Add("p_capacity", Capacity);
            cmd.Parameters.Add("p_type", "PRESENTATION_ROOM");
            cmd.Parameters.Add("p_id_location", Location.Id);
            cmd.Parameters.Add("p_id_organizer", Organiser.Id);
            cmd.Parameters.Add("p_vc_ready", null);
            cmd.Parameters.Add("p_podium_size", PodiumSize);

            cmd.ExecuteNonQuery();
        }
    }
}
