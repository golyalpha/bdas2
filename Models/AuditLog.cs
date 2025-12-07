using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string TableName { get; set; }
    public string Operation { get; set; }
    public int? RecordId { get; set; }
    public int? OrganizerId { get; set; }
    public string OrganizerName { get; set; }
    public string ChangedAt { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }

    public static List<AuditLog> GetRecentLogs(int limit = 100, string tableName = null)
    {
        var list = new List<AuditLog>();
        
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "admin_pkg.get_audit_log";
            
            cmd.Parameters.Add(new OracleParameter
            {
                ParameterName = "p_limit",
                OracleDbType = OracleDbType.Int32,
                Direction = System.Data.ParameterDirection.Input,
                Value = limit
            });
            
            cmd.Parameters.Add(new OracleParameter
            {
                ParameterName = "p_table_name",
                OracleDbType = OracleDbType.Varchar2,
                Direction = System.Data.ParameterDirection.Input,
                Value = string.IsNullOrEmpty(tableName) ? (object)DBNull.Value : tableName
            });
            
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
                list.Add(new AuditLog
                {
                    Id = reader.GetInt32(0),
                    TableName = reader.GetString(1),
                    Operation = reader.GetString(2),
                    RecordId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    OrganizerId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    OrganizerName = reader.GetString(5),
                    ChangedAt = reader.GetString(6),
                    OldValues = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    NewValues = reader.IsDBNull(8) ? "" : reader.GetString(8)
                });
            }
        }
        
        return list;
    }
}