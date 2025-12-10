using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class AuditLog
{
    public int IdLog { get; set; }
    public string TableName { get; set; }
    public string Operation { get; set; }
    public int? RecordId { get; set; }
    public string OrganizerName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    /// <summary>
    /// Získá stránkovaný seznam audit záznamù pomocí Oracle funkce
    /// </summary>
    public static PagedResult<AuditLog> GetPagedAuditLogs(
        int pageNumber = 1,
        int pageSize = 50,
        string? tableName = null,
        string? operation = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var logs = new List<AuditLog>();
        int totalCount = 0;

        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using (OracleCommand cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = @"
                    SELECT 
                        id_log, 
                        table_name, 
                        operation, 
                        record_id, 
                        organizer_name, 
                        changed_at, 
                        old_values, 
                        new_values,
                        total_count
                    FROM TABLE(admin_pkg.get_audit_page(
                        p_page_number => :p_page_number,
                        p_page_size => :p_page_size,
                        p_table_name => :p_table_name,
                        p_operation => :p_operation,
                        p_date_from => :p_date_from,
                        p_date_to => :p_date_to
                    ))";

                cmd.Parameters.Add("p_page_number", pageNumber);
                cmd.Parameters.Add("p_page_size", pageSize);
                cmd.Parameters.Add("p_table_name", string.IsNullOrEmpty(tableName) ? (object)DBNull.Value : tableName.ToUpper());
                cmd.Parameters.Add("p_operation", string.IsNullOrEmpty(operation) ? (object)DBNull.Value : operation.ToUpper());
                cmd.Parameters.Add("p_date_from", dateFrom ?? (object)DBNull.Value);
                cmd.Parameters.Add("p_date_to", dateTo ?? (object)DBNull.Value);

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (totalCount == 0 && !reader.IsDBNull(8))
                        {
                            totalCount = reader.GetInt32(8);
                        }

                        logs.Add(new AuditLog
                        {
                            IdLog = reader.GetInt32(0),
                            TableName = reader.GetString(1),
                            Operation = reader.GetString(2),
                            RecordId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                            OrganizerName = reader.GetString(4),
                            ChangedAt = reader.GetDateTime(5),
                            OldValues = reader.IsDBNull(6) ? null : reader.GetString(6),
                            NewValues = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }
        }

        return new PagedResult<AuditLog>
        {
            Items = logs,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}