using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class NotificationType
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public required string Code { get; set; }

    [Required]
    [StringLength(50)]
    public required string Name { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public static NotificationType GetNotificationType(int id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"SELECT id_notification_type, ""code"", ""name"", ""description"" 
                               FROM notification_types WHERE id_notification_type = :id";
            cmd.Parameters.Add("id", id);
            
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                throw new KeyNotFoundException($"NotificationType with ID {id} not found.");
            }

            return new NotificationType
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
    }

    public static NotificationType? GetNotificationTypeByCode(string code)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"SELECT id_notification_type, ""code"", ""name"", ""description"" 
                               FROM notification_types WHERE ""code"" = :code";
            cmd.Parameters.Add("code", code);
            
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new NotificationType
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
    }

    public static List<NotificationType> ListNotificationTypes()
    {
        var list = new List<NotificationType>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"SELECT id_notification_type, ""code"", ""name"", ""description"" 
                               FROM notification_types ORDER BY id_notification_type";
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new NotificationType
                {
                    Id = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    Name = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
        }
        return list;
    }
}