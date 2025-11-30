using Oracle.ManagedDataAccess.Client;
using WebApp.Util;

namespace WebApp.Models;

public class OrganizerHierarchy
{
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int? SubstituteId { get; set; }
    public string HierarchyPath { get; set; }
    public string TopOrganizer { get; set; }

    public static List<OrganizerHierarchy> GetHierarchy()
    {
        var list = new List<OrganizerHierarchy>();
        
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            
            // ZMÌNA: Volání stored procedury místo VIEW
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "reservations_pkg.get_organizers_hierarchy";
            
            // OUT parametr s kurzorem
            var cursorParam = new OracleParameter
            {
                ParameterName = "p_cursor",
                OracleDbType = OracleDbType.RefCursor,
                Direction = System.Data.ParameterDirection.Output
            };
            cmd.Parameters.Add(cursorParam);
            
            cmd.ExecuteNonQuery();
            
            // Ètení dat z kurzoru
            using var reader = ((Oracle.ManagedDataAccess.Types.OracleRefCursor)cursorParam.Value).GetDataReader();
            while (reader.Read())
            {
                list.Add(new OrganizerHierarchy
                {
                    Level = reader.GetInt32(0),
                    Id = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    SubstituteId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    HierarchyPath = reader.GetString(5),
                    TopOrganizer = reader.GetString(6)
                });
            }
        }
        
        return list;
    }
}