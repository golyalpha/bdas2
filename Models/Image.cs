using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.ComponentModel.DataAnnotations;
using WebApp.Util;

namespace WebApp.Models;

public class Image
{
    public int Id { get; set; }

    [Required]
    public byte[] Data { get; set; }

    [StringLength(255)]
    public string? FileName { get; set; }

    [StringLength(10)]
    public string? FileSuffix { get; set; }

    public DateTime? CreatedAt { get; set; }

    [Required]
    public int IdRoom { get; set; }

    [Required]
    public int IdOrganizer { get; set; }

    [Required]
    public int IdLocation { get; set; }


    public void Persist()
    {
        Console.WriteLine($"[DEBUG] Starting Persist for Room {IdRoom}");
        Console.WriteLine($"[DEBUG] Data length: {Data?.Length ?? 0} bytes");
        
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    OracleCommand cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;

                    // Smažeme existující obrázek
                    cmd.CommandText = "DELETE FROM images WHERE id_room = :id_room";
                    cmd.Parameters.Add("id_room", IdRoom);
                    int deleted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"[DEBUG] Deleted {deleted} old images");
                    cmd.Parameters.Clear();

                    // ZMĚNA: Vložíme data přímo pomocí OracleBlob parametru
                    cmd.CommandText = @"INSERT INTO images (""data"", file_name, file_suffix, created_at, id_organizer, id_location, id_room) 
                                       VALUES (:blob_data, :file_name, :file_suffix, :created_at, :id_organizer, :id_location, :id_room) 
                                       RETURNING id_image INTO :id_image";
                    
                    // BLOB parametr - přímo vložíme data
                    var blobParam = new OracleParameter("blob_data", OracleDbType.Blob)
                    {
                        Value = Data
                    };
                    cmd.Parameters.Add(blobParam);
                    
                    cmd.Parameters.Add("file_name", FileName);
                    cmd.Parameters.Add("file_suffix", FileSuffix);
                    cmd.Parameters.Add("created_at", CreatedAt.Value);
                    cmd.Parameters.Add("id_organizer", IdOrganizer);
                    cmd.Parameters.Add("id_location", IdLocation);
                    cmd.Parameters.Add("id_room", IdRoom);
                    
                    var idParam = new OracleParameter("id_image", OracleDbType.Int32, System.Data.ParameterDirection.Output);
                    cmd.Parameters.Add(idParam);
                    
                    cmd.ExecuteNonQuery();
                    Id = ((OracleDecimal)idParam.Value).ToInt32();
                    
                    Console.WriteLine($"[DEBUG] Inserted image with ID: {Id} directly (no BLOB update needed)");

                    transaction.Commit();
                    Console.WriteLine($"[DEBUG] Transaction committed successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Exception in Persist: {ex.Message}");
                    Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public static Image? GetImageByRoom(int roomId)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT id_image, ""data"", file_name, file_suffix, created_at, id_organizer, id_location, id_room 
                               FROM images_v WHERE id_room = :id_room";
            cmd.Parameters.Add("id_room", roomId);

            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    return null;
                }

                OracleBlob blob = reader.GetOracleBlob(1);
                byte[] data = new byte[blob.Length];
                blob.Read(data, 0, (int)blob.Length);
                blob.Close();

                return new Image
                {
                    Id = reader.GetInt32(0),
                    Data = data,
                    FileName = reader.GetString(2),
                    FileSuffix = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    IdOrganizer = reader.GetInt32(5),
                    IdLocation = reader.GetInt32(6),
                    IdRoom = reader.GetInt32(7)
                };
            }
        }
    }


    public void Delete()
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM images WHERE id_image = :id";
            cmd.Parameters.Add("id", Id);
            cmd.ExecuteNonQuery();
        }
    }

    public static bool HasImage(int roomId)
    {
        using (OracleConnection conn = DBManager.GetConnection())
        {
            conn.Open();
            OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM images_v WHERE id_room = :id_room";
            cmd.Parameters.Add("id_room", roomId);
            
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }

/// <summary>
/// Načte všechny obrázky s daty z více tabulek pro zobrazení v seznamu
/// </summary>
public static List<ImageViewModel> ListAllImagesWithDetails()
{
    var list = new List<ImageViewModel>();
    using (OracleConnection conn = DBManager.GetConnection())
    {
        conn.Open();
        OracleCommand cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                id_image, 
                file_name, 
                file_suffix, 
                created_at,
                id_room,
                room_name,
                location_name,
                city_name,
                organizer_name
            FROM images_with_detail_v
            ORDER BY created_at DESC";
        cmd.CommandType = System.Data.CommandType.Text;
        
        using (OracleDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                list.Add(new ImageViewModel
                {
                    Id = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    FileSuffix = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    RoomId = reader.GetInt32(4),
                    RoomName = reader.GetString(5),
                    LocationName = reader.GetString(6),
                    CityName = reader.GetString(7),
                    OrganizerName = reader.GetString(8)
                });
            }
        }
    }
    return list;
    }
}
