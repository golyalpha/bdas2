using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Util;

namespace WebApp.Models;

public class Image
{
    public int Id { get; set; }
    public byte[] Data { get; set; }
    public int IdRoom { get; set; }
    public int IdOrganizer { get; set; }
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

                    // Vložíme nový záznam s EMPTY_BLOB()
                    cmd.CommandText = @"INSERT INTO images (""data"", id_organizer, id_location, id_room) 
                                       VALUES (EMPTY_BLOB(), :id_organizer, :id_location, :id_room) 
                                       RETURNING id_image INTO :id_image";
                    
                    cmd.Parameters.Add("id_organizer", IdOrganizer);
                    cmd.Parameters.Add("id_location", IdLocation);
                    cmd.Parameters.Add("id_room", IdRoom);
                    
                    var idParam = new OracleParameter("id_image", OracleDbType.Int32, System.Data.ParameterDirection.Output);
                    cmd.Parameters.Add(idParam);
                    
                    cmd.ExecuteNonQuery();
                    Id = ((OracleDecimal)idParam.Value).ToInt32();
                    Console.WriteLine($"[DEBUG] Inserted new image with ID: {Id}");

                    // Nyní aktualizujeme BLOB data
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"SELECT ""data"" FROM images WHERE id_image = :id_image FOR UPDATE";
                    cmd.Parameters.Add("id_image", Id);

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            OracleBlob blob = reader.GetOracleBlob(0);
                            Console.WriteLine($"[DEBUG] BLOB opened, writing {Data.Length} bytes...");
                            blob.Write(Data, 0, Data.Length);
                            blob.Close();
                            Console.WriteLine($"[DEBUG] BLOB write successful!");
                        }
                        else
                        {
                            Console.WriteLine($"[ERROR] Failed to read BLOB for update!");
                            throw new Exception("Failed to read BLOB for update");
                        }
                    }

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
            cmd.CommandText = @"SELECT id_image, ""data"", id_organizer, id_location, id_room 
                               FROM images WHERE id_room = :id_room";
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
                    IdOrganizer = reader.GetInt32(2),
                    IdLocation = reader.GetInt32(3),
                    IdRoom = reader.GetInt32(4)
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
            cmd.CommandText = "SELECT COUNT(*) FROM images WHERE id_room = :id_room";
            cmd.Parameters.Add("id_room", roomId);
            
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }
    }
}