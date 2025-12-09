using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class Notification
{
    public int Id { get; set; }
    public required NotificationType NotificationType { get; set; }
    public required bool Delivered { get; set; }
    public required Organiser Organiser { get; set; }
    public required RoomRequest Request { get; set; }

    /// <summary>
    /// ZÌsk· poËet nep¯eËten˝ch notifikacÌ pomocÌ PL/SQL funkce
    /// </summary>
    public static int GetUnreadCount(int organiserId)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT notifications_pkg.get_unread_notification_count(:id_organizer) FROM DUAL";
            cmd.Parameters.Add("id_organizer", organiserId);
            
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }

    public static Notification GetNotification(int id)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"SELECT id_notification, delivered, id_organizer, id_room_request, id_notification_type 
                               FROM notifications_v WHERE id_notification = :id";
            cmd.Parameters.Add("id", id);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                throw new KeyNotFoundException($"Notification with ID {id} not found.");
            }

            return new Notification
            {
                Id = reader.GetInt32(0),                                              // id_notification
                Delivered = reader.GetString(1) == "Y",                               // delivered
                Organiser = Organiser.GetOrganiser(reader.GetInt32(2)),              // id_organizer
                Request = RoomRequest.GetRequest(reader.GetInt32(3)),                 // id_room_request
                NotificationType = NotificationType.GetNotificationType(reader.GetInt32(4))  // id_notification_type
            };
        }
    }

    public static List<Notification> ListByOrganiserId(int organiserId)
    {
        var list = new List<Notification>();
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"SELECT id_notification, delivered, id_organizer, id_room_request, id_notification_type 
                               FROM notifications 
                               WHERE id_organizer = :id 
                               ORDER BY id_notification DESC";
            cmd.Parameters.Add("id", organiserId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Notification
                {
                    Id = reader.GetInt32(0),                                              // id_notification
                    Delivered = reader.GetString(1) == "Y",                               // delivered
                    Organiser = Organiser.GetOrganiser(reader.GetInt32(2)),              // id_organizer
                    Request = RoomRequest.GetRequest(reader.GetInt32(3)),                 // id_room_request
                    NotificationType = NotificationType.GetNotificationType(reader.GetInt32(4))  // id_notification_type
                });
            }
        }
        return list;
    }

    public static void MarkAsDelivered(int organiserId, int[] notificationIds)
    {
        if (notificationIds == null || notificationIds.Length == 0)
        {
            return;
        }

        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            
            // Vol·nÌ procedury pro KAéD… ID zvl·öù
            foreach (var notificationId in notificationIds)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "notifications_pkg.mark_notification_delivered"; 
                
                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_id_organizer",
                    OracleDbType = OracleDbType.Int32,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = organiserId
                });
                
                cmd.Parameters.Add(new OracleParameter
                {
                    ParameterName = "p_notification_id",
                    OracleDbType = OracleDbType.Int32,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = notificationId
                });
                
                cmd.ExecuteNonQuery();
            }
        }
    }
}
