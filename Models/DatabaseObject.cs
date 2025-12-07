using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class DatabaseObject
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string CreatedDate { get; set; }
    public string LastModified { get; set; }

    public static List<DatabaseObject> GetAllObjects()
    {
        var list = new List<DatabaseObject>();
        
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "admin_pkg.list_database_objects";
            
            var cursorParam = new OracleParameter
            {
                ParameterName = "p_cursor",
                OracleDbType = OracleDbType.RefCursor,
                Direction = System.Data.ParameterDirection.Output
            };
            cmd.Parameters.Add(cursorParam);
            
            cmd.ExecuteNonQuery();
            
            using var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader();
            while (reader.Read())
            {
                list.Add(new DatabaseObject
                {
                    Type = reader.GetString(0),
                    Name = reader.GetString(1),
                    Status = reader.GetString(2),
                    CreatedDate = reader.GetString(3),
                    LastModified = reader.GetString(4)
                });
            }
        }
        
        return list;
    }
}